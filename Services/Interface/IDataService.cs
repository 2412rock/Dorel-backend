using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Requests;
using DorelAppBackend.Models.Responses;

namespace DorelAppBackend.Services.Interface
{
    public interface IDataService
    {
        public Maybe<DBJudetModel[]> GetJudete(string startsWith);
        public Maybe<DBServiciuModel[]> GetServicii(string startsWith);

        public Task<List<JunctionServiciuJudete>> GetAllJunctions();
        public Maybe<DBServiciuModel[]> GetServiciiForUser(string email, bool ofer);
        public Task<Maybe<string>> AssignServiciu(string userEmail, int serviciuId, int[] judeteIds, string descriere, Imagine[] imagini, bool ofer);

        public Maybe<List<DBJudetModel>> GetJudeteForServiciu(int serviciuId, string userEmail);

        public Task<Maybe<List<Imagine>>> GetImaginiServiciuUser(int serviciuId, string userEmail, bool ofer);

        public Maybe<string> GetDescriereForServiciu(int serviciuId, string userEmail);

        public Task<Maybe<string>> EditServiciu(string userEmail, int serviciuId, int[] judeteIds, string descriere, Imagine[] imagini, bool ofer);

        public Task<Maybe<string>> DeleteUserServiciu(string userEmail, int serviciuId, bool ofer);

        public Task<Maybe<List<SearchResultResponse>>> GetServiciiForJudet(int serviciuId, int judetId, int pageNumber, bool offer);

        public Task<Maybe<List<Imagine>>> GetImaginiForServiciuOfUser(int serviciuId, int judetId, int userId, bool ofer);

        public Task<Maybe<List<SearchResultResponse>>> GetServiciiForUserAsSearchResults(string email, bool ofer);

        public Task<Maybe<List<string>>> GetAllUsers();
    }
}
