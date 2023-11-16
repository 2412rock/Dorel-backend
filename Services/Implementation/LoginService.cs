using Azure;
using DorelAppBackend.Enums;
using DorelAppBackend.Models;
using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DorelAppBackend.Services.Implementation
{
    public class LoginService : ILoginService
    {
        private LoginDbContext loginDbContext;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMailService _mailService;
        private readonly IPasswordHashService _passwordHashService;
        public LoginService(LoginDbContext loginDbContext,
            IRedisCacheService redisCacheService,
            IMailService mailService,
            IPasswordHashService passwordHashService)
        {
            _redisCacheService = redisCacheService;
            _mailService = mailService;
            _passwordHashService = passwordHashService;
            this.loginDbContext = loginDbContext;
        }


        public void LoginGoogle(string email, string name)
        {
            var userExists = loginDbContext.Users.Any(e => e.Email == email);
            if (!userExists)
            {
                loginDbContext.Users.Add(new UserLoginInfoModel() { Email = email, Password = "", Name = name});
                loginDbContext.SaveChanges();
            }
        }

        public LoginEnum LoginUser(string email, string password)
        {
            LoginEnum response;
            var user = loginDbContext.Users.Where(e => e.Email == email).FirstOrDefault();
            if(user != null)
            {
                var hashPassword = user.Password;
                if( _passwordHashService.VerifyPassword(password, hashPassword)){
                    response = LoginEnum.LoginSuccess;
                }
                else
                {
                    response = LoginEnum.InvalidPassword;
                }
            }
            else
            {
                response = LoginEnum.UserDoesNotExist;
            }

            return response;
        }

        public VerifyUserEnum VerifyUser(string email, string verificationCode)
        {
            var redisValue = _redisCacheService.GetValueFromCache(email);

            if(redisValue != null)
            {
                var verificationObject = JsonSerializer.Deserialize<UserVerification>(redisValue);

                if(verificationObject.VerificationCode == verificationCode)
                {
                    var userExists = loginDbContext.Users.Any(e => e.Email == email);

                    if (!userExists)
                    {
                        loginDbContext.Users.Add(new UserLoginInfoModel() { Email = email, Password = verificationObject.Password, Name = verificationObject.Name });
                        loginDbContext.SaveChanges();
                        _redisCacheService.RemoveValueFromCache(email);

                        return VerifyUserEnum.VerificationSuccesful;
                    }

                    _redisCacheService.RemoveValueFromCache(email);

                    return VerifyUserEnum.UserAlreadyRegistered;
                }

                return VerifyUserEnum.VerificationCodeInvalid;
            }

            return VerifyUserEnum.EmailDoesNotExist;
        }

        public void SendVerification(string email, string password, string name)
        {
            try
            {
                if (_redisCacheService.GetValueFromCache(email) != null)
                {
                    _redisCacheService.RemoveValueFromCache(email);
                }
                var verificationCode = new Random().Next(1000, 9999).ToString();

                var data = new UserVerification()
                {
                    Name = name,
                    Password = _passwordHashService.HashPassword(password),
                    VerificationCode = verificationCode
                };

                var dataSerial = JsonSerializer.Serialize(data);
                _mailService.SendMailToUser(verificationCode, email);
                _redisCacheService.SetValueInCache(email, dataSerial);
            }

            catch
            {
                throw;
            }
        }

        public void DeleteVerification(string email)
        {
            _redisCacheService.RemoveValueFromCache(email);
        }
    }
}
