using StackExchange.Redis;

namespace ProxyCache.ServiceCollectionExtensions
{
    public static class RedisServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var sentinelConnectionString = configuration.GetValue<string>("Redis:SentinelConfiguration");
                if (string.IsNullOrWhiteSpace(sentinelConnectionString))
                    throw new ArgumentNullException(nameof(sentinelConnectionString), "The Sentinel node cannot be empty");
                var sentinelOptions = ConfigurationOptions.Parse(sentinelConnectionString);
                sentinelOptions.AbortOnConnectFail = false;
                sentinelOptions.CommandMap = CommandMap.Default;
                sentinelOptions.TieBreaker = "";
                var redisOptions = sentinelOptions.Clone();
                var sentinelConnect = ConnectionMultiplexer.SentinelConnect(sentinelOptions);
                var masterConnection = sentinelConnect.GetSentinelMasterConnection(redisOptions);
                return masterConnection;
            });
        }
    }
}
