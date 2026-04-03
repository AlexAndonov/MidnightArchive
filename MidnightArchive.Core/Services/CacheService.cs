using Microsoft.Extensions.Caching.Distributed;
using MidnightArchive.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MidnightArchive.Core.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache cache;

        public CacheService(IDistributedCache _cache)
        {
            cache = _cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedData = await cache.GetStringAsync(key);

            if (cachedData == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task RemoveAsync(string key)
        {
            await cache.RemoveAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            var serializedData = JsonSerializer.Serialize(value);

            await cache.SetStringAsync(key, serializedData, options);
        }
    }
}
