using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Responses;
using DorelAppBackend.Services.Interface;
using Microsoft.EntityFrameworkCore;

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
            var existingLog = await _dbContext.AccessLogs.FirstOrDefaultAsync(e => e.IpAddress == ipAddress);
            if (existingLog == null)
            {
                await _dbContext.AccessLogs.AddAsync(new Models.DbModels.DBAccessLog() { AccessTime = DateTime.Now, IpAddress = ipAddress });

            }
            else
            {
                existingLog.AccessTime = DateTime.Now;
                _dbContext.AccessLogs.Update(existingLog);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Maybe<List<DBAccessLog>>> GetLogs()
        {
            var list = await _dbContext.AccessLogs.ToListAsync();
            var maybe = new Maybe<List<DBAccessLog>>();
            maybe.SetSuccess(list);
            return maybe;
        }
    }
}
