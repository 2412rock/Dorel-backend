using DorelAppBackend.Services.Interface;

namespace DorelAppBackend.Services.Implementation
{
    public class AccessLogsService: IAccessLogsService
    {
        private readonly DorelDbContext _dbContext;
        public AccessLogsService(DorelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddLog(string ipAddress)
        {
            await _dbContext.AccessLogs.AddAsync(new Models.DbModels.DBAccessLog() { AccessTime = DateTime.Now, IpAddress = ipAddress });
            await _dbContext.SaveChangesAsync();
        }
    }
}
