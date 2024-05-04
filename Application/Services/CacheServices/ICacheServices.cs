using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CacheServices
{
    public interface ICacheServices
    {
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        bool TryGetValue<T>(string key, out T result);
        Task<T?> GetAsync<T>(string key);
        Task UpdateAsync<T>(string key, T newValue, TimeSpan? expiry = null);
        Task ClearAsync(string key);

    }
}
