using DorelAppBackend.Services.Interface;
using StackExchange.Redis;
using System.Net;

namespace DorelAppBackend.Services.Implementation
{
    public class RedisService: IRedisService
    {
        private readonly ConnectionMultiplexer _redisConnection;

        public RedisService()
        {

            string hostIp = "192.168.1.159";
            

            var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { $"{hostIp}:6379" },
                Password = redisPassword,
                // Add other configuration options as needed
                // For example, configurationOptions.Ssl = true; for SSL/TLS
            };

            _redisConnection = ConnectionMultiplexer.Connect(configurationOptions);
        }

        public IDatabase GetDatabase()
        {
            return _redisConnection.GetDatabase();
        }
    }
}
