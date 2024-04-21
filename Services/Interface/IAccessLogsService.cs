using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Responses;

namespace DorelAppBackend.Services.Interface
{
    public interface IAccessLogsService
    {
        public Task AddLog(string ipAddress);

        public Task<Maybe<List<DBAccessLog>>> GetLogs();
    }
}
