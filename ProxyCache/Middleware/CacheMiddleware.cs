using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using ProxyCache.Common;
using ProxyCache.Models;
using Serilog;
using StackExchange.Redis;
using System.Net;

namespace ProxyCache.Middleware
{
    //
    // 摘要:
    //     Adds Reverse Cache routes to the route table using the default processing pipeline.
    public static class CachingMiddlewareExtensions
    {
        public static IApplicationBuilder UseFileCaching(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FileCacheMiddleware>();
        }
        public static IApplicationBuilder UseMemoryCaching(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MemoryCacheMiddleware>();
        }
    }
    //
    // 摘要:
    //     Extension methods for Microsoft.AspNetCore.Routing.IEndpointRouteBuilder used
    //     to add Reverse MemoryCache to the ASP .NET Core request pipeline.
    public class MemoryCacheMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var url = context.Request.GetEncodedUrl();
                var key = FileExtensions.MD5Create(url);
                if (!cache.TryGetValue(key, out CachedResponseData? cachedResponse))
                {
                    var uager = context.Request.Headers.UserAgent;
                    context.Response.Headers["X-Static-Page"] = "MISS";
                    var responseStream = context.Response.Body;
                    using var buffer = new MemoryStream();
                    context.Response.Body = buffer;
                    await next(context);
                    cachedResponse = new CachedResponseData(context.Response.Headers, await FileExtensions.CompressGZip(buffer.ToArray()));
                    if (context.Response.IsSuccessStatusCode())
                        cache.Set(key, cachedResponse, TimeSpan.FromMinutes(1));
                    buffer.Position = 0;
                    await buffer.CopyToAsync(responseStream);
                }
                else
                {
                    if (cachedResponse is null)
                    {
                        context.Response.Headers.CacheControl = "public, max-age=2592000";
                        context.Response.Headers.ContentType = "text/html; charset=utf-8";
                        context.Response.Headers["X-Static-Page"] = "HIT";
                    }
                    else
                    {
                        foreach (var header in cachedResponse.Headers)
                        {
                            context.Response.Headers[header.Key] = header.Value;
                        }
                    }
                    context.Response.Headers["X-Static-Page"] = "HIT";
                    await context.Response.Body.WriteAsync(await FileExtensions.DecompressGZip(cachedResponse?.Body ?? []));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                await next(context);
            }
        }
    }
    //
    // 摘要:
    //     Extension methods for Microsoft.AspNetCore.Routing.IEndpointRouteBuilder used
    //     to add Reverse FileCache to the ASP .NET Core request pipeline.
    public class FileCacheMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var url = context.Request.GetEncodedUrl();
                var key = FileExtensions.MD5Create(url);
                if (!File.Exists(key))
                {
                    var uager = context.Request.Headers.UserAgent;
                    context.Response.Headers["X-Static-Page"] = "MISS";
                    var responseStream = context.Response.Body;
                    using var buffer = new MemoryStream();
                    context.Response.Body = buffer;
                    await next(context);
                    if (context.Response.IsSuccessStatusCode())
                    {
                        string rootpath = configuration.GetSection("CachePath").Get<string>() ?? string.Empty;
                        key = Path.Combine(rootpath,key);
                        await FileExtensions.CreateHtml(key, buffer.ToArray(), true);
                    }
                    buffer.Position = 0;
                    await buffer.CopyToAsync(responseStream);
                }
                else
                {
                    context.Response.Headers.CacheControl = "public, max-age=2592000";
                    context.Response.Headers.ContentType = "text/html; charset=utf-8";
                    context.Response.Headers["X-Static-Page"] = "HIT";
                    context.Response.Headers.ContentEncoding = "gzip";
                    var body = await FileExtensions.Gethtmlbyte(key, true);
                    await context.Response.Body.WriteAsync(body);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                await next(context);
            }
        }
    }
}
