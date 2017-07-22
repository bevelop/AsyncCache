using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace AsyncCache
{
    public class AsyncCache<T>
    {
        readonly IMemoryCache _cache;
        readonly TimeSpan _expiration;

        public AsyncCache(TimeSpan expiration)
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _expiration = expiration;
        }

        public async Task<T> AddOrGetExisting(string key, Func<Task<T>> valueFactory)
        {
            var newEntry = new CacheEntry<T>(valueFactory);
            var existingItem = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpiration = DateTime.UtcNow.Add(_expiration);
                return newEntry;
            });

            try
            {
                return await (existingItem ?? newEntry).Get();
            }
            catch (Exception)
            {
                _cache.Remove(key);
                throw;
            }
        }
    }
}
