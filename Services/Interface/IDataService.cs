﻿using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Models.Responses;

namespace DorelAppBackend.Services.Interface
{
    public interface IDataService
    {
        public Maybe<DBJudetModel[]> GetJudete(string startsWith);
        public Maybe<DBServiciuModel[]> GetServicii(string startsWith);

        public Maybe<DBServiciuModel[]> GetServiciiForUser(string email);
        public Task<Maybe<string>> AssignServiciu(string userEmail, int serviciuId, int[] judeteIds, string descriere, Imagine[] imagini);


    }
}
