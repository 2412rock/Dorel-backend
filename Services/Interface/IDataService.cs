using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Responses;

namespace DorelAppBackend.Services.Interface
{
    public interface IDataService
    {
        public Maybe<DBJudetModel[]> GetJudete(string startsWith);
        public Maybe<DBServiciuModel[]> GetServicii(string startsWith);

    }
}
