//using Core.Application.Common;
//using Core.Domain.DTOs.Shared;
//using Entities.UsersEntity;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Core.Application.Services.AuthorizeServices;

//public interface IAuthServices
//{
//    Task<IServiceResponse> RegisterUser(RegisterDTO Request);
//    Task<IServiceResponse<TokenDTO>> CheckPhoneCodeAsync(CheckCodeDTO input);
//    Task<IServiceResponse<TokenDTO>> LoginUser(LoginWithPhoneDTO Request);
//    Task<IServiceResponse<bool>> CheckUserExist(string PhoneNumber);
//    Task<IServiceResponse> ForgotPassword(string PhoneNumber);
//    Task<IServiceResponse> ReSendCode(string PhoneNumber);
//    Task<IServiceResponse> ReNewPassword(string Password, int UserId);
//    Task<IServiceResponse> LoginOrRegisterUser(RegisterDTO Request);
//}

//public class AuthServices : IAuthServices
//{

//    private readonly IGenericRepository<User> _UserRepo;
//    private readonly IGenericRepository<VerificationCode> _VerificationCodeRepo;
//    private readonly IToken _token;

//    public AuthServices(IGenericRepository<User> userRepo, IGenericRepository<VerificationCode> verificationCodeRepo, IToken token)
//    {
//        _UserRepo = userRepo;
//        _VerificationCodeRepo = verificationCodeRepo;
//        _token = token;
//    }

//    public async Task<bool> LoginUserAsAdmin(LoginUserDTO Request)
//    {
//        var UserExist = await _UserRepo.FirstOrDefualt(x => x.Email == Request.Email);

//        var verify = HashGenerator.VerifyHashPassword(Request.Password, UserExist.PasswordSalt, UserExist.PasswordHash);
//        if (verify)
//        {
//            return true;
//        }
//        else
//        {
//            return false;
//        }
//    }

//    public async Task<bool> InsertAdmin(InsertUserDTO Request)
//    {
//        HashGenerator.MakeHmacHashCode(Request.Password, out byte[] passwordHash, out byte[] passwordSalt);
//        var UserExist = _UserRepo.Any(x => x.Email == Request.Email);
//        if (!UserExist)
//        {
//            try
//            {
//                await _UserRepo.Insert(new User
//                {
//                    RoleId = Request.RoleId,
//                    MarketerId = Request.MarketerId,
//                    PhoneNumber = Request.PhoneNumber,
//                    Name = Request.Name,
//                    Email = Request.Email,
//                    PasswordHash = passwordHash,
//                    PasswordSalt = passwordSalt,
//                    IsActive = true
//                });
//                await _UserRepo.SaveAsync();

//                return true;
//            }
//            catch (Exception e)
//            {
//                return false;
//            }
//        }
//        else
//        {
//            return false;
//        }
//    }

//    public async Task<ServiceResponse> RegisterHostUser(RegisterHostDTO Request)
//    {
//        ServiceResponse result = new ServiceResponse() { IsSuccess = true };

//        var UserExist = _UserRepo.Any(x => x.PhoneNumber == Request.PhoneNumber);
//        if (!UserExist)
//        {
//            try
//            {
//                HashGenerator.MakeHmacHashCode(Request.PhoneNumber, out byte[] passwordHash, out byte[] passwordSalt);

//                // insert user
//                await _UserRepo.Insert(new User
//                {
//                    RoleId = 2,
//                    Name = Request.Name,
//                    Email = Request.Email,
//                    PhoneNumber = Request.PhoneNumber,
//                    PasswordHash = passwordHash,
//                    PasswordSalt = passwordSalt,
//                    IsActive = false
//                });
//                await _UserRepo.SaveAsync();

//            }
//            catch (Exception e)
//            {
//                result.IsSuccess = false;
//                result.ErrorMessage = "خطایی در هنگام ثبت نام رخ داده است. لطفا با پشتیبانی تماس حاصل فرمایید.";
//            }
//        }
//        else
//        {
//            result.IsSuccess = false;
//            result.ErrorMessage = "شما قبلا ثبت نام کرده‌اید.";
//        }

//        return result;
//    }

//    public async Task<ServiceResponse> EditAdmin(RegisterDTO Request)
//    {
//        ServiceResponse result = new ServiceResponse() { IsSuccess = true };

//        try
//        {
//            HashGenerator.MakeHmacHashCode(Request.Password, out byte[] passwordHash, out byte[] passwordSalt);

//            // Update user
//            _UserRepo.Update(new User
//            {
//                Id = Request.Id.Value,
//                RoleId = 3,
//                Status = Request.Status.HasValue ? Request.Status.Value : null,
//                SellerId = Request.SellerId.HasValue ? Request.SellerId.Value : null,
//                CreateDate = DateTime.Now,
//                MarketerId = Request.MarketerId.HasValue ? Request.MarketerId.Value : null,
//                Name = Request.Name,
//                PhoneNumber = Request.PhoneNumber,
//                PasswordHash = passwordHash,
//                PasswordSalt = passwordSalt,
//                IsActive = Request.IsActive.HasValue ? Request.IsActive.Value : false
//            });
//            await _UserRepo.SaveAsync();

//            result.IsSuccess = true;
//            return result;

//        }
//        catch (Exception ex)
//        {
//            result.ErrorMessage = ex.Message;
//            result.IsSuccess = false;
//            return result;
//        }

//    }
//    public async Task<ServiceResponse> RegisterUser(RegisterDTO Request)
//    {
//        ServiceResponse result = new ServiceResponse() { IsSuccess = true };

//        var UserExist = _UserRepo.Any(x => x.PhoneNumber == Request.PhoneNumber);
//        if (!UserExist)
//        {
//            try
//            {
//                HashGenerator.MakeHmacHashCode(Request.Password, out byte[] passwordHash, out byte[] passwordSalt);

//                // insert user
//                await _UserRepo.Insert(new User
//                {
//                    RoleId = 3,
//                    Status = Request.Status.HasValue ? Request.Status.Value : null,
//                    SellerId = Request.SellerId.HasValue ? Request.SellerId.Value : null,
//                    CreateDate = DateTime.Now,
//                    MarketerId = Request.MarketerId.HasValue ? Request.MarketerId.Value : null,
//                    Name = !String.IsNullOrEmpty(Request.Name) ? Request.Name : "مشتری",
//                    PhoneNumber = Request.PhoneNumber,
//                    PasswordHash = passwordHash,
//                    PasswordSalt = passwordSalt,
//                    IsActive = Request.IsActive.HasValue ? Request.IsActive.Value : false
//                });
//                await _UserRepo.SaveAsync();


//                //generate number
//                Random generator = new Random();
//                int r = generator.Next(1000, 9999);

//                if (!Request.IsInsertedByMarketer)
//                {
//                    //send sms 
//                    Dictionary<string, string> values = new Dictionary<string, string>()
//                        {
//                          { "verification-code", r.ToString() }
//                        };
//                    var Smsrequest = new FarazSMSSendByPatternDTO
//                    {

//                        code = "6pmpkr09no36brn",
//                        recipient = Request.PhoneNumber,
//                        sender = "+983000505",
//                        variable = values
//                    };
//                    var smsRes = await FarazSMSSender.SendByPatternAsync(Smsrequest);
//                    if (!smsRes.IsSuccess)
//                    {
//                        result.IsSuccess = false;
//                        result.ErrorMessage = smsRes.ErrorMessage;
//                    }
//                    else
//                    {
//                        //save verification code in db
//                        var vc = new VerificationCode()
//                        {
//                            Code = r.ToString(),
//                            Date = DateTime.UtcNow,
//                            Phone = Request.PhoneNumber,
//                            Status = 0 //sended
//                        };
//                        await _VerificationCodeRepo.Insert(vc);
//                        await _VerificationCodeRepo.SaveAsync();

//                    }
//                }




//            }
//            catch (Exception e)
//            {
//                result.IsSuccess = false;
//                result.ErrorMessage = "خطایی در هنگام ثبت نام رخ داده است. لطفا با پشتیبانی تماس حاصل فرمایید.";
//            }
//        }
//        else
//        {
//            result.IsSuccess = false;
//            result.ErrorMessage = "این شماره قبلا ثبت نام شده است.";
//        }

//        return result;
//    }

//    public async Task<ServiceResponse> LoginOrRegisterUser(RegisterDTO Request)
//    {
//        ServiceResponse result = new ServiceResponse() { IsSuccess = true };
//        byte[] passhash = null;
//        byte[] passsalt = null;

//        var UserExist = _UserRepo.Any(x => x.PhoneNumber == Request.PhoneNumber);
//        if (!UserExist)
//        {
//            if (!string.IsNullOrEmpty(Request.Password))
//            {
//                HashGenerator.MakeHmacHashCode(Request.Password, out byte[] passwordHash, out byte[] passwordSalt);
//                passhash = passwordHash;
//                passsalt = passwordSalt;
//            }
//            try
//            {
//                // insert user
//                await _UserRepo.Insert(new User
//                {
//                    RoleId = 3,
//                    Status = Request.Status.HasValue ? Request.Status.Value : null,
//                    SellerId = Request.SellerId.HasValue ? Request.SellerId.Value : null,
//                    CreateDate = DateTime.Now,
//                    MarketerId = Request.MarketerId.HasValue ? Request.MarketerId.Value : null,
//                    Name = !string.IsNullOrEmpty(Request.Name) ? Request.Name : "کاربر",
//                    PhoneNumber = Request.PhoneNumber,
//                    PasswordHash = !string.IsNullOrEmpty(Request.Password) ? passhash : null,
//                    PasswordSalt = !string.IsNullOrEmpty(Request.Password) ? passsalt : null,
//                    IsActive = Request.IsActive.HasValue ? Request.IsActive.Value : false
//                });
//                await _UserRepo.SaveAsync();
//            }
//            catch (Exception ex)
//            {

//                throw;
//            }

//        }

//        try
//        {
//            //generate number
//            Random generator = new Random();
//            int r = generator.Next(1000, 9999);

//            //send sms 
//            Dictionary<string, string> values = new Dictionary<string, string>()
//                    {
//                        { "verification-code", r.ToString() }
//                    };
//            var Smsrequest = new FarazSMSSendByPatternDTO
//            {

//                code = "6pmpkr09no36brn",
//                recipient = Request.PhoneNumber,
//                sender = "+983000505",
//                variable = values
//            };
//            var smsRes = await FarazSMSSender.SendByPatternAsync(Smsrequest);
//            if (!smsRes.IsSuccess)
//            {
//                result.IsSuccess = false;
//                result.ErrorMessage = smsRes.ErrorMessage;
//            }
//            else
//            {
//                //save verification code in db
//                var vc = new VerificationCode()
//                {
//                    Code = r.ToString(),
//                    Date = DateTime.UtcNow,
//                    Phone = Request.PhoneNumber,
//                    Status = 0 //sended
//                };
//                await _VerificationCodeRepo.Insert(vc);
//                await _VerificationCodeRepo.SaveAsync();
//            }
//        }
//        catch (Exception e)
//        {
//            result.IsSuccess = false;
//            result.ErrorMessage = "خطایی در هنگام ثبت نام رخ داده است. لطفا با پشتیبانی تماس حاصل فرمایید.";
//        }


//        return result;
//    }

//    public async Task<ServiceResponse<TokenDTO>> CheckPhoneCodeAsync(CheckCodeDTO Request)
//    {
//        Request.PhoneNumber = Request.PhoneNumber.Trim();
//        Request.Code = Request.Code.Trim();

//        ServiceResponse<TokenDTO> result = new ServiceResponse<TokenDTO>() { IsSuccess = true };
//        //check code
//        VerificationCode code = await _VerificationCodeRepo.FirstOrDefualt(x => x.Phone == Request.PhoneNumber && x.Code == Request.Code && x.Status == 0);

//        //check code validity
//        if (code == null)
//        {
//            result.IsSuccess = false;
//            result.ErrorMessage = "کد اشتباه است!";
//            return result;
//        }
//        else if (code.Date.AddMinutes(15) <= DateTime.UtcNow)
//        {
//            result.IsSuccess = false;
//            result.ErrorMessage = "کد منقضی شده است!";
//            return result;
//        }

//        //set code as used
//        code.Status = 1;
//        _VerificationCodeRepo.Update(code);
//        await _VerificationCodeRepo.SaveAsync();

//        var user = await _UserRepo.FirstOrDefualt(x => x.PhoneNumber == Request.PhoneNumber);

//        if (user != null)
//        {
//            //generate token
//            result.Data = await _token.Get(user);
//            result.Data.FullName = user.Name;
//        }
//        else
//        {
//            result.IsSuccess = false;
//            result.ErrorMessage = "لطفا ابتدا ثبت نام نمایید.";
//        }
//        return result;
//    }

//    public async Task<ServiceResponse<TokenDTO>> LoginUser(LoginWithPhoneDTO Request)
//    {
//        ServiceResponse<TokenDTO> result = new ServiceResponse<TokenDTO>() { IsSuccess = true };

//        var UserExist = await _UserRepo.FirstOrDefualt(x => x.PhoneNumber == Request.PhoneNumber);
//        if (UserExist == null)//check user exist
//        {
//            result.ErrorMessage = "کاربری با این نام وجود ندارد";
//            result.IsSuccess = false;
//        }
//        else
//        {
//            var verify = HashGenerator.VerifyHashPassword(Request.Password, UserExist.PasswordSalt, UserExist.PasswordHash);
//            if (verify && UserExist.IsActive == true)
//            {
//                //generate token
//                var token = await _token.Get(UserExist);
//                token.FullName = UserExist.Name;
//                result.IsSuccess = true;
//                result.Data = token;
//            }
//            else if (UserExist.IsActive == false)
//            {
//                result.ErrorMessage = "کاربر فعال نمی‌باشد ";
//                result.IsSuccess = false;
//            }
//            else
//            {
//                result.ErrorMessage = "شماره تلفن یا پسورد اشتباه است.";
//                result.IsSuccess = false;
//            }
//        }

//        return result;
//    }

//    public async Task<ServiceResponse<bool>> CheckUserExist(string PhoneNumber)
//    {
//        ServiceResponse<bool> result = new ServiceResponse<bool>() { IsSuccess = true };

//        var UserExist = await _UserRepo.FirstOrDefualt(x => x.PhoneNumber == PhoneNumber);
//        if (UserExist == null)//check user exist
//        {
//            result.Data = false;
//        }
//        else
//        {
//            result.Data = true;
//        }

//        return result;
//    }

//    public async Task<ServiceResponse> ForgotPassword(string PhoneNumber)
//    {
//        PhoneNumber = PhoneNumber.Trim();

//        var result = new ServiceResponse() { IsSuccess = true, ErrorMessage = "" };
//        var user = await _UserRepo.FirstOrDefualt(x => x.PhoneNumber == PhoneNumber);

//        if (user != null)
//        {
//            //generate number
//            Random generator = new Random();
//            int r = generator.Next(1000, 9999);

//            //send sms 
//            Dictionary<string, string> values = new Dictionary<string, string>()
//                    {
//                        { "verification-code", r.ToString() }
//                    };
//            var Smsrequest = new FarazSMSSendByPatternDTO
//            {

//                code = "6pmpkr09no36brn",
//                recipient = PhoneNumber,
//                sender = "+983000505",
//                variable = values
//            };
//            var smsRes = await FarazSMSSender.SendByPatternAsync(Smsrequest);
//            if (!smsRes.IsSuccess)
//            {
//                result.IsSuccess = false;
//                result.ErrorMessage = smsRes.ErrorMessage;
//            }
//            else
//            {
//                //save verification code in db
//                var vc = new VerificationCode()
//                {
//                    Code = r.ToString(),
//                    Date = DateTime.Now,
//                    Phone = PhoneNumber,
//                    Status = 0 //sended
//                };
//                await _VerificationCodeRepo.Insert(vc);
//                await _VerificationCodeRepo.SaveAsync();
//            }
//        }
//        else
//        {
//            result.IsSuccess = true;
//            result.ErrorMessage = "کد تایید ارسال شد.";
//        }

//        return result;
//    }

//    public async Task<ServiceResponse> ReNewPassword(string Password, int UserId)
//    {
//        var result = new ServiceResponse() { IsSuccess = true, ErrorMessage = "" };
//        var user = await _UserRepo.FirstOrDefualt(x => x.Id == UserId);

//        if (user != null)
//        {
//            HashGenerator.MakeHmacHashCode(Password, out byte[] passwordHash, out byte[] passwordSalt);
//            user.PasswordHash = passwordHash;
//            user.PasswordSalt = passwordSalt;

//            _UserRepo.Update(user);
//            await _UserRepo.SaveAsync();
//        }
//        else
//        {
//            result.IsSuccess = false;
//            result.ErrorMessage = "کاربری با مشخصات شما پیدا نشد. لطفا مجددا تلاش نمایید.";
//        }

//        return result;
//    }

//    public async Task<ServiceResponse> ReSendCode(string PhoneNumber)
//    {
//        PhoneNumber = PhoneNumber.Trim();

//        var result = new ServiceResponse() { IsSuccess = true, ErrorMessage = "" };
//        var user = await _UserRepo.FirstOrDefualt(x => x.PhoneNumber == PhoneNumber);

//        if (user != null)
//        {
//            //generate number
//            Random generator = new Random();
//            int r = generator.Next(1000, 9999);

//            //send sms 
//            Dictionary<string, string> values = new Dictionary<string, string>()
//                    {
//                        { "verification-code", r.ToString() }
//                    };
//            var Smsrequest = new FarazSMSSendByPatternDTO
//            {

//                code = "6pmpkr09no36brn",
//                recipient = PhoneNumber,
//                sender = "+983000505",
//                variable = values
//            };
//            var smsRes = await FarazSMSSender.SendByPatternAsync(Smsrequest);
//            if (!smsRes.IsSuccess)
//            {
//                result.IsSuccess = false;
//                result.ErrorMessage = smsRes.ErrorMessage;
//            }
//            else
//            {
//                //save verification code in db
//                var vc = new VerificationCode()
//                {
//                    Code = r.ToString(),
//                    Date = DateTime.Now,
//                    Phone = PhoneNumber,
//                    Status = 0 //sended
//                };
//                await _VerificationCodeRepo.Insert(vc);
//                await _VerificationCodeRepo.SaveAsync();
//            }
//        }
//        else
//        {
//            result.IsSuccess = true;
//            result.ErrorMessage = "کد تایید ارسال شد.";
//        }

//        return result;
//    }

//}

