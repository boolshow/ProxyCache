using Serilog;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace ProxyCache.Common
{
    public class FileExtensions
    {
        //
        // 摘要:
        //     Character string changed to md5 encrypted character.
        //
        // 返回结果:
        //     MD5 encrypted character string.
        //     
        public static string MD5Create(string key)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(key);
            byte[] hashBytes = MD5.HashData(inputBytes);
            string hashed = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hashed;
        }
        //
        // 摘要:
        //     Compress the specified byte[].
        //     
        // 参数:
        //   compressedData:
        //     byte[] to write uncompressed data.
        //     
        // 返回值:
        //     compressed byte[].
        //         
        public static async Task<byte[]> CompressGZip(byte[] data)
        {
            using MemoryStream memoryStream = new();
            using GZipStream gzipStream = new(memoryStream, CompressionMode.Compress);
            await gzipStream.WriteAsync(data);
            await gzipStream.FlushAsync();
            return memoryStream.ToArray();
        }
        //
        // 摘要:
        //     Unzip the specified byte[].
        //     
        // 参数:
        //   compressedData:
        //     The byte[] to which compressed data is written. 
        //     
        // 返回值:
        //    Decompressed byte[].
        //        
        public static async Task<byte[]> DecompressGZip(byte[] compressedData)
        {
            try
            {
                using MemoryStream compressedStream = new(compressedData);
                using GZipStream gzipStream = new(compressedStream, CompressionMode.Decompress);
                using MemoryStream decompressedStream = new();
                await gzipStream.CopyToAsync(decompressedStream);
                return decompressedStream.ToArray();
            }
            catch
            {
                return [];
            }
        }
        /// <summary>
        /// 写入文件操作
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="bytes"></param>
        /// <param name="compressGzip">是否进行压缩</param>
        /// <returns></returns>
        public static async Task CreateHtml(string filename, byte[] bytes, bool compressGzip = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filename))
                    return;
                if (compressGzip)
                    bytes = await CompressGZip(bytes);
                string? directoryPath = Path.GetDirectoryName(filename);
                if (string.IsNullOrWhiteSpace(directoryPath))
                    return;
                Directory.CreateDirectory(directoryPath);
                using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write, 40960);
                await fileStream.WriteAsync(bytes);
                await fileStream.FlushAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                Console.WriteLine(ex.ToString());
            }
        }
        /// <summary>
        /// 读取文件操作
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="compressGzip"></param>
        /// <returns></returns>
        public static async Task<byte[]> Gethtmlbyte(string filename, bool compressGzip = false)
        {
            byte[] result;
            if (!File.Exists(filename))
                return [];
            using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 40960);
            var buffer = new byte[4096];
            using var memoryStream = new MemoryStream();
            int bytesRead;
            while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
            {
                memoryStream.Write(buffer, 0, bytesRead);
            }
            result = memoryStream.ToArray();
            if (compressGzip)
                result = await DecompressGZip(result);
            return result;
        }

        
    }
}
