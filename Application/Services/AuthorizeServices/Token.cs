
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Application.Database;
using Application.Extensions;
using Application.Services.AuthorizeServices.DTO;
using Domain.DTOs.Shared;
using Domain.Entities.Roles;
using Domain.Entities.Users;

namespace Application.Services.AuthorizeServices
{

    public interface IToken
    {

        public Task<TokenDTO> Get(User user, CancellationToken cancellationToken = default);
        public Task<IServiceResponse<TokenDTO>> RefreshAsync(RefreshTokenDTO tokenApiModel, CancellationToken cancellationToken = default);
    }
    public class Token : IScopedDependency, IToken
    {
        #region Constructor
        private readonly IConfiguration configuration;
        private readonly ILogger<Token> _log;
        private readonly IUnitOfWork _unitOfWork;
        public Token(
            IConfiguration _configuration,
            ILogger<Token> log,
            IUnitOfWork unitOfWork)
        {
            configuration = _configuration;
            _log = log;
            _unitOfWork = unitOfWork;
        }
        #endregion

        private async Task<string> GetRoleNameByRoleId(long id)
        {
            return await _unitOfWork.GetRepository<Role>().GetAsNoTrackingQuery().Where(x => x.Id == id).Select(z => z.RoleName).FirstOrDefaultAsync();
        }

        public async Task<TokenDTO> Get(User user, CancellationToken cancellationToken = default)
        {
            var _UserRepo = _unitOfWork.GetRepository<User>();
            if (user != null)
            {
                var roleName = await GetRoleNameByRoleId(user.RoleId);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.MobilePhone, user.Mobile),
                   new Claim(ClaimTypes.Role, roleName)
                };

                var token = GenerateJwtToken(authClaims);
                var refreshToken = GenerateRefreshToken();

                user.IsActive = true;
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(int.Parse(configuration["JWT:RefreshTokenValidityInDays"]) + 7);

                await _UserRepo.UpdateAsync(user, cancellationToken);
                await _unitOfWork.CommitAsync();

                var tk = new TokenDTO
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    CreateDate = DateTime.Now,
                    RefreshTokenCreateDate = DateTime.Now,
                    ExpireDate = token.ValidTo,
                    RefreshTokenExpireDate = user.RefreshTokenExpiryTime.Value
                };
                return tk;
            }
            return null;
        }

        public async Task<IServiceResponse<TokenDTO>> RefreshAsync(RefreshTokenDTO tokenApiModel, CancellationToken cancellationToken = default)
        {
            var result = new ServiceRespnse<TokenDTO>() { IsSuccess = false };
            var _UserRepo = _unitOfWork.GetRepository<User>();
            if (tokenApiModel is null)
            {
                result.Message = "Invalid Inputs";
                return result;
            }
            string accessToken = tokenApiModel.Token;
            string refreshToken = tokenApiModel.RefreshToken;
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var username = principal.FindFirstValue(ClaimTypes.NameIdentifier); //this is mapped to the Name claim by default
            bool convSuccess = int.TryParse(username, out int userId);

            if (!convSuccess)
            {
                result.Message = "Invalid Inputs";
                return result;
            }

            var user = await _UserRepo.GetAsNoTrackingQuery()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                result.Message = "Login Again";
                return result;
            }

            var newAccessToken = GenerateJwtToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(int.Parse(configuration["JWT:RefreshTokenValidityInDays"]) + 7);

            await _UserRepo.UpdateAsync(user, cancellationToken);
            await _unitOfWork.CommitAsync();

            result.IsSuccess = true;
            result.Data = new TokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                RefreshToken = newRefreshToken,
                ExpireDate = newAccessToken.ValidTo,
                RefreshTokenExpireDate = user.RefreshTokenExpiryTime.Value
            };
            return result;
        }


        #region utilities

        private JwtSecurityToken GenerateJwtToken(List<Claim> claims)
        {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(int.Parse(configuration["JWT:TokenValidityInDays"]));

            var token = new JwtSecurityToken(
                configuration["JWT:ValidIssuer"],
                configuration["JWT:ValidAudience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }
        #endregion

    }

}
