namespace DorelAppBackend.Services.Interface
{
    public interface IMailService
    {
        public void SendMailToUser(string verificationCode, string recipientEmail);
    }
}
