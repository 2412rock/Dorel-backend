using StackExchange.Redis;

namespace DorelAppBackend.Services.Interface
{
    public interface IRedisService
    {
        public IDatabase GetDatabase();
    }
}
