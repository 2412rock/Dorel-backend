namespace DorelAppBackend.Services.Interface
{
    public interface IAccessLogsService
    {
        public Task AddLog(string ipAddress);
    }
}
