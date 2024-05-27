
using ProxyCache.Common;
using ProxyCache.Middleware;
using ProxyCache.ServiceCollectionExtensions;
using Serilog;

namespace ProxyCache
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
            builder.Services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });
            var reverseProxyConfigSection = builder.Configuration.GetSection("ReverseProxy");
            if (reverseProxyConfigSection.Exists())
            {
                builder.Services.AddReverseProxy().LoadFromConfig(reverseProxyConfigSection);
            }
            builder.Services.AddMemoryCache();

            builder.Services.AddRedisCache(builder.Configuration);

            builder.Services.AddSingleton<RedisSubscriber>();

            var app = builder.Build();

            app.UseMemoryCaching();

            //app.UseFileCaching();

            app.MapReverseProxy();

            await app.RunAsync();
        }
    }
}
