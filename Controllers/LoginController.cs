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
        [Route("api/sendVerification")]
        public IActionResult RegisterPreVertified([FromBody] RegisterUserRequest request)
        {
            loginService.SendVerification(request.Email, request.Password, request.Name);
            return Ok(new LoginResponse() { Message = "Verification sent succesfully" });
        }

        [HttpPost]
        [Route("api/verifyUser")]
        public IActionResult VerifyUser([FromBody] VerifyUserRequest request)
        {
            Console.WriteLine("GOT REQUEST TO VERIFY USER");
            var result = loginService.VerifyUser(request.Email, request.VerificationCode);

            switch (result)
            {
                case VerifyUserEnum.VerificationSuccesful:
                    return Ok(new LoginResponse() { Message = "Verification success" });

                case VerifyUserEnum.UserAlreadyRegistered:
                    return BadRequest(new LoginResponse() { Message = "User already registered" });

                case VerifyUserEnum.EmailDoesNotExist:
                    return BadRequest(new LoginResponse() { Message = "Email does not exist" });

                case VerifyUserEnum.VerificationCodeInvalid:
                    return BadRequest(new LoginResponse() { Message = "Verification code invalid" });
            }

            return NotFound(new LoginResponse() { Message = "Not found" });
        }
    }
}
