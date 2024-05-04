using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.AuthorizeServices.DTO
{
    public class TokenDTO
    {
        public string RefreshToken { get; set; }
        public string Token { get; set; }
        public string FullName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime RefreshTokenCreateDate { get; set; }
        public DateTime RefreshTokenExpireDate { get; set; }
    }
    public class RefreshTokenDTO
    {
        public string RefreshToken { get; set; }
        public string Token { get; set; }
    }
}
