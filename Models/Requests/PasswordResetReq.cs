﻿namespace DorelAppBackend.Models.Requests
{
    public class PasswordResetReq
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public string Code { get; set; }
    }
}
