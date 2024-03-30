using DorelAppBackend.Models.DbModels;
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

        public Maybe<List<DBJudetModel>> GetJudeteForServiciu(int serviciuId, string userEmail);

        public Task<Maybe<List<Imagine>>> GetImaginiServiciuUser(int serviciuId, string userEmail);

        public Maybe<string> GetDescriereForServiciu(int serviciuId, string userEmail);

        public Task<Maybe<string>> EditServiciu(string userEmail, int serviciuId, int[] judeteIds, string descriere, Imagine[] imagini);

        public Maybe<string> DeleteUserServiciu(string userEmail, int serviciuId);

        public Task<Maybe<List<SearchResultResponse>>> GetServiciiForJudet(int serviciuId, int judetId, int pageNumber);

        public Task<Maybe<List<Imagine>>> GetImaginiForServiciuOfUser(int serviciuId, int judetId, int userId);
    }
}
