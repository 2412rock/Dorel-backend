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
        public LoginService(LoginDbContext loginDbContext,
            IRedisCacheService redisCacheService,
            IMailService mailService)
        {
            _redisCacheService = redisCacheService;
            _mailService = mailService;
            this.loginDbContext = loginDbContext;
        }

        public bool LoginUser(string username, string password)
        {
            throw new NotImplementedException();
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
                    Password = password,
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
