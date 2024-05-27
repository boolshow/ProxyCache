using Microsoft.Extensions.Primitives;

namespace ProxyCache.Common
{
    public static class HttpResponseExtensions
    {
        //
        // 摘要:
        //     Gets a value that indicates if the HTTP response was successful.
        //
        // 返回结果:
        //     true if Microsoft.AspNetCore.Http.HttpResponse.StatusCode was in the range 200-299;
        //     otherwise, false.
        public static bool IsSuccessStatusCode(this HttpResponse response)
        {
            return response.StatusCode >= 200 && response.StatusCode <= 299;
        }
        //
        // 摘要:
        //     Gets a value indicating whether useragent is in the blacklist.
        //
        // 参数:
        //   Blacklist:
        //      UserAgenT Blacklist.
        //
        // 返回结果:
        //     If Microsoft. AspNetCore. Http. HttpRequest. Headers. UserAgent is beyond the scope of the blacklist, it is true;
        //     otherwise, false.
        public static bool ExitUserAgenT(this StringValues ua, IEnumerable<string> Blacklist)
        {
            if (string.IsNullOrWhiteSpace(ua) || ua.Count <= 0)
                return true;
            return Blacklist.Any(x => x.Equals(ua, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
