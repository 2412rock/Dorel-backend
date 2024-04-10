using DorelAppBackend.Enums;
using DorelAppBackend.Models.Responses;

namespace DorelAppBackend.Services.Interface
{
    public interface ILoginService
    {
        public Task<Maybe<string>> SendVerification(string email, string password, string name);

        public Maybe<string[]> LoginUser(string email, string password);

        public Task<Maybe<string[]>> LoginGoogle(string email, string name, string idToken);

        public Maybe<string> VerifyUser(string email, string verificationCode);

        public Maybe<string> RefreshToken(string token);

    }
}
