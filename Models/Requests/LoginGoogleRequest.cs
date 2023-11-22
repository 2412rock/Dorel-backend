namespace DorelAppBackend.Models.Requests
{
    public class LoginGoogleRequest
    {
        public string Email { get; set; }

        public string Name { get; set; }

        public string IdToken { get; set; }
    }
}
