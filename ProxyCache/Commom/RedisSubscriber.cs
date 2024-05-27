using Serilog;
using StackExchange.Redis;
using System.Text.Json;

namespace ProxyCache.Common
{
    public class RedisSubscriber
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IConfiguration _configuration;
        public RedisSubscriber(IConnectionMultiplexer redisConnection, IConfiguration configuration)
        {
            _redisConnection = redisConnection;
            _configuration = configuration;
            try
            {
                var key = new RedisChannel("DelCahe", RedisChannel.PatternMode.Auto);
                var subscriber = _redisConnection.GetSubscriber();
                subscriber.SubscribeAsync(key, (channel, message) =>
                {
                    string? mess = message;
                    if (!string.IsNullOrEmpty(mess))
                    {
                        var data = JsonSerializer.Deserialize<CaheMc>(mess);
                        if (data is not null)
                        {
                            Uri uri = new(data.Url);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, ex.Message);
            }
        }
    }
    public class CaheMc
    {
        public string Url { get; set; } = string.Empty;
        public int recursive { get; set; } = 0;
    }
}
