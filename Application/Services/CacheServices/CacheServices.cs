using Application.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace Application.Services.CacheServices
{
    public class CacheServices : ICacheServices, IScopedDependency
    {
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _serializerOptions;
        public CacheServices(IDistributedCache cache)
        {
            _cache = cache;
            _serializerOptions = new();
        }



        public async Task UpdateAsync<T>(string key, T newValue, TimeSpan? expiry = null)
        {
            byte[] existingData = await _cache.GetAsync(key);

            if (existingData == null)
            {
                throw new KeyNotFoundException($"The key '{key}' does not exist in the cache.");
            }

            byte[] newData = JsonSerializer.SerializeToUtf8Bytes(newValue, _serializerOptions);

            await _cache.SetAsync(key, newData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
        }

        public bool TryGetValue<T>(string key, out T result)
        {
            var json = _cache.GetString(key);
            if (json == null)
            {
                result = default;
                return false;
            }
            result = JsonSerializer.Deserialize<T>(json);
            return true;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            byte[] data = JsonSerializer.SerializeToUtf8Bytes(value, _serializerOptions);

            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiry;
            }

            await _cache.SetAsync(key, data, options);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            // Get the data from the cache
            byte[] data = await _cache.GetAsync(key);

            if (data == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(data, _serializerOptions);
        }


        public async Task ClearAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
