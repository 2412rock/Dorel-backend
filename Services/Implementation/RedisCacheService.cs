using DorelAppBackend.Services.Interface;
using StackExchange.Redis;

namespace DorelAppBackend.Services.Implementation
{
    public class RedisCacheService: IRedisCacheService
    {
        private readonly IDatabase _redisDatabase;
        private readonly IRedisService _redisService;

        public RedisCacheService(IRedisService redisService)
        {
            _redisService = redisService;
            _redisDatabase = redisService.GetDatabase();
        }

        public string GetValueFromCache(string key)
        {
            return _redisDatabase.StringGet(key);
        }

        public void SetValueInCache(string key, string value)
        {
            TimeSpan expiry = new TimeSpan(0, 0, 10, 0);
            _redisDatabase.StringSet(key, value, expiry);
        }

        public void RemoveValueFromCache(string key)
        {
            _redisDatabase.KeyDelete(key);
        }
    }
}
