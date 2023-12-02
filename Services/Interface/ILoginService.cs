using DorelAppBackend.Enums;
using DorelAppBackend.Models.Responses;

namespace DorelAppBackend.Services.Interface
{
    public interface ILoginService
    {
        public Maybe<string> SendVerification(string email, string password, string name);

        public Maybe<string[]> LoginUser(string email, string password);

        public Maybe<string[]> LoginGoogle(string email, string name, string idToken);

        public Maybe<string> VerifyUser(string email, string verificationCode);

        public string GetEmailFromToken(string token);
    }
}
