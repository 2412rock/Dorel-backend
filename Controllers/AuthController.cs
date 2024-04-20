﻿using DorelAppBackend.Enums;
using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services;
using DorelAppBackend.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DorelAppBackend.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private ILoginService loginService;
        public AuthController(ILoginService loginService)
        {
            this.loginService = loginService;
        }

        [HttpPost]
        [Route("api/refreshToken")]
        public IActionResult LoginGoogle([FromBody] RefreshRequest request)
        {
            var result = loginService.RefreshToken(request.RefreshToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/loginGoogle")]
        public async Task<IActionResult> LoginGoogle(LoginGoogleRequest request)
        {
            var result = await loginService.LoginGoogle(request.Email, request.Name, request.IdToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var response = loginService.LoginUser(request.Email, request.Password);
            return Ok(response);
        }

        [HttpPost]
        [Route("api/sendVerification")]
        public async Task<IActionResult> RegisterPreVertified([FromBody] RegisterUserRequest request)
        {
            var result = await loginService.SendVerification(request.Email, request.Password, request.Name);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/verifyUser")]
        public IActionResult VerifyUser([FromBody] VerifyUserRequest request)
        {
            var result = loginService.VerifyUser(request.Email, request.VerificationCode);

            return Ok(result);
        }

        [HttpPost]
        [Route("api/setResetPasswordCode")]
        public async Task<IActionResult> SetResetPasswordCode([FromBody] PasswordResetCodeReq request)
        {
            var result = await loginService.SendPasswordResetVerificationCode(request.Email);

            return Ok(result);
        }

        [HttpPost]
        [Route("api/resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetReq req)
        {
            var result = await loginService.ResetPassword(req.Email, req.Code, req.Password);

            return Ok(result);
        }
    }
}