﻿using DorelAppBackend.Enums;

namespace DorelAppBackend.Services.Interface
{
    public interface ILoginService
    {
        public void SendVerification(string email, string password, string name);

        public LoginEnum LoginUser(string email, string password);

        public VerifyUserEnum VerifyUser(string email, string verificationCode);
    }
}
