using StackExchange.Redis;

namespace ParcelManagement.Infrastructure.Database
{
    public interface IRedisConnectionFactory
    {
        IDatabase GetDatabase();
    }

    public class RedisConnectionFactory : IRedisConnectionFactory, IDisposable
    {
        private readonly Lazy<ConnectionMultiplexer> _connection;

        public RedisConnectionFactory(string redisConnectionString)
        {
            _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redisConnectionString));
        }

        public void Dispose()
        {
            if (_connection.IsValueCreated)
            {
                _connection.Value.Dispose();
            }
        }

        public IDatabase GetDatabase()
        {
            return _connection.Value.GetDatabase();
        }
    }
}