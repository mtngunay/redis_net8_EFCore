using StackExchange.Redis;
using System;
using System.Threading.Tasks;


namespace RedisApi.Services
{
    public class CacheService
    {
        private readonly IConnectionMultiplexer _redis;

        public CacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var db = _redis.GetDatabase();
            var valueString = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            await db.StringSetAsync(key, valueString, expiration);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var db = _redis.GetDatabase();
            var valueString = await db.StringGetAsync(key);
            if (string.IsNullOrEmpty(valueString))
            {
                return default;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(valueString);
        }

        public async Task RemoveAsync(string key)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
    }
}
