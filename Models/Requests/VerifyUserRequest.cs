using System.ComponentModel.DataAnnotations;

namespace DorelAppBackend.Models.Requests
{
    public class VerifyUserRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string VerificationCode { get; set; }
    }
}
