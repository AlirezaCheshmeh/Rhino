using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Utility
{
    public static class CacheExtension
    {
        public static async Task UpdateCacheAsync(IDistributedCache cache, string key, object newData, TimeSpan? expiration = null)
        {
            byte[] data = JsonSerializer.SerializeToUtf8Bytes(newData);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            await cache.SetAsync(key, data, options);
        }
        private static IDistributedCache _disCache;
        public static void Initialize(IDistributedCache distributedCache)
        {
            _disCache = distributedCache;
        }
        public static async Task<bool> UpdateValueAsync<T>(string key, T? input)
        {
            try
            {
                if (input is null) return false;
                await _disCache.RemoveAsync(key);
                var json = JsonSerializer.Serialize(input);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _disCache.SetAsync(key, bytes);
                return true;
            }
            catch (Exception e)
            {
                //todo: log
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<T?> GetValueAsync<T>(string key)
        {
            try
            {
                var cacheData = await _disCache.GetAsync(key);
                var res = JsonSerializer.Deserialize<T>(cacheData);
                return res ?? default; // todo : show suitable message to client.
            }
            catch (Exception ex)
            {
                //todo: log
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
