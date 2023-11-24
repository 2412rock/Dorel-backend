using DorelAppBackend.Enums;
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
    public class LoginController : ControllerBase
    {
        private ILoginService loginService;
        public LoginController(ILoginService loginService)
        {
            this.loginService = loginService;
        }

        [HttpPost]
        [Route("api/loginGoogle")]
        public IActionResult LoginGoogle(LoginGoogleRequest request)
        {
            var result = loginService.LoginGoogle(request.Email, request.Name, request.IdToken);
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
        public IActionResult RegisterPreVertified([FromBody] RegisterUserRequest request)
        {
            var result = loginService.SendVerification(request.Email, request.Password, request.Name);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/verifyUser")]
        public IActionResult VerifyUser([FromBody] VerifyUserRequest request)
        {
            Console.WriteLine("GOT REQUEST TO VERIFY USER");
            var result = loginService.VerifyUser(request.Email, request.VerificationCode);

            return Ok(result);
        }
    }
}
