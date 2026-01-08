using ParcelManagement.Core.Repositories;
using ParcelManagement.Infrastructure.Database;
using StackExchange.Redis;

namespace ParcelManagement.Infrastructure.Repository
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IDatabase _redis; 

        public RedisRepository(IRedisConnectionFactory connectionFactory)
        {
            _redis = connectionFactory.GetDatabase();
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var val = await _redis.StringGetAsync(key);
            return val.ToString();
        }

        public async Task<bool> KeyExistAsync(string key)
        {
            return await _redis.KeyExistsAsync(key);
        }

        public async Task<bool> SetValueAsync(string key, string value, TimeSpan timeToLive)
        {
            return await _redis.StringSetAsync(key, value, timeToLive);
        }
    }
}