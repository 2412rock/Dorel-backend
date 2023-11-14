using System.ComponentModel.DataAnnotations;

namespace DorelAppBackend.Models
{
    public class UserVerification
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get;set; }

        [Required]
        public string VerificationCode { get;set; }
    }
}
