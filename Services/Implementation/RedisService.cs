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
            string hostIp = "";
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses("host.docker.internal");
                if (addresses.Length > 0)
                {
                    // we are running in docker
                    hostIp = addresses[0].ToString();
                }
                else
                {
                    // running locally
                    hostIp = Environment.GetEnvironmentVariable("HOST_IP");
                }
            }
            catch
            {
                hostIp = "redis-server";
            }

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
