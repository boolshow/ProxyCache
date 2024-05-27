using Microsoft.Extensions.Primitives;

namespace ProxyCache.Models
{
    public class CachedResponseData
    {
        public Dictionary<string, StringValues> Headers { get; set; }
        public byte[] Body { get; set; }

        public CachedResponseData(IDictionary<string, StringValues> headers, byte[] body)
        {
            Headers = new Dictionary<string, StringValues>(headers);
            Body = body;
        }
    }
}
