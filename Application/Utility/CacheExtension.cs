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
    }
}
