using Azure;
using DorelAppBackend.Enums;
using DorelAppBackend.Models;
using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace DorelAppBackend.Services.Implementation
{
    public class LoginService : ILoginService
    {
        private DorelDbContext dorelDbContext;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMailService _mailService;
        private readonly IPasswordHashService _passwordHashService;
        public LoginService(DorelDbContext dorelDbContext,
            IRedisCacheService redisCacheService,
            IMailService mailService,
            IPasswordHashService passwordHashService)
        {
            _redisCacheService = redisCacheService;
            _mailService = mailService;
            _passwordHashService = passwordHashService;
            this.dorelDbContext = dorelDbContext;
        }

        public string GenerateJwtToken(string userEmail, bool isRefreshToken=false)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            byte[] bytes = Encoding.UTF8.GetBytes(secretKey);
            string base64String = Convert.ToBase64String(bytes);
            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(base64String));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var issuer = "dorelapp.xyz";
            var audience = "endpoint";
            var claims = !isRefreshToken ? new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userEmail), // User identifier from Google
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique JWT ID
                new Claim(JwtRegisteredClaimNames.Iss, issuer), // Token issuer
                new Claim(JwtRegisteredClaimNames.Aud, audience), // Token audience
            } : new[] 
            {
                new Claim(JwtRegisteredClaimNames.Sub, userEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: isRefreshToken ? DateTime.UtcNow.AddHours(24) : DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }


        public Maybe<string[]> LoginGoogle(string email, string name, string idToken)
        {
            var response = new Maybe<string[]>();
            var userExists = dorelDbContext.Users.Any(e => e.Email == email);
            if (VerifyGoogleToken(idToken))
            {
                if (!userExists)
                {
                    dorelDbContext.Users.Add(new DBUserLoginInfoModel() { Email = email, Password = "", Name = name });
                    dorelDbContext.SaveChanges();
                }
                response.SetSuccess(new string[] {GenerateJwtToken(email, true), GenerateJwtToken(email) });
            }
            else
            {
                response.SetException("Invalid google token");
            }
            return response;
        }

        private bool VerifyGoogleToken(string idToken)
        {
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings()).Result;
                if(payload != null)
                {
                    return true;
                }
            }
            catch (InvalidJwtException ex)
            {
                Console.WriteLine("Invalid google token");
            }
            return false;
        }

        public Maybe<string[]> LoginUser(string email, string password)
        {
            var response = new Maybe<string[]>();
            var user = dorelDbContext.Users.Where(e => e.Email == email).FirstOrDefault();
            if(user != null)
            {
                var hashPassword = user.Password;
                if( _passwordHashService.VerifyPassword(password, hashPassword)){
                    response.SetSuccess(new string[] { GenerateJwtToken(email, true), GenerateJwtToken(email) });
                }
                else
                {
                    response.SetException("Invalid user password");
                }
            }
            else
            {
                response.SetException("User does not exist");
            }

            return response;
        }

        public Maybe<string> VerifyUser(string email, string verificationCode)
        {
            var result = new Maybe<string>();
            var redisValue = _redisCacheService.GetValueFromCache(email);

            if (redisValue != null)
            {
                var verificationObject = JsonSerializer.Deserialize<UserVerification>(redisValue);

                if (verificationObject.VerificationCode == verificationCode)
                {
                    var userExists = dorelDbContext.Users.Any(e => e.Email == email);

                    if (!userExists)
                    {
                        dorelDbContext.Users.Add(new DBUserLoginInfoModel() { Email = email, Password = verificationObject.Password, Name = verificationObject.Name });
                        dorelDbContext.SaveChanges();

                        result.SetSuccess("Verification ok");
                    }
                    else
                    {
                        result.SetException("User already exists");
                    }

                    _redisCacheService.RemoveValueFromCache(email);

                }
                else
                {
                    result.SetException("Code invalid");
                }
            }
            else
            {
                result.SetException("Email does not exist");
            }

            return result;
        }

        public async Task<Maybe<string>> SendVerification(string email, string password, string name)
        {
            var result = new Maybe<string>();
            var userExists = await dorelDbContext.Users.AnyAsync(u => u.Email == email);
            if (userExists)
            {
                result.SetException("Email already in use");
                return result;
            }
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
                result.SetSuccess("Verification sent succesfully");
            }

            catch(Exception ex)
            {

                result.SetException(ex.Message);
            }
            return result;
        }

        public void DeleteVerification(string email)
        {
            _redisCacheService.RemoveValueFromCache(email);
        }
    }
}
