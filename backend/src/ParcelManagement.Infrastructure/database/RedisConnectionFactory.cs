using Microsoft.Extensions.Options;
using ParcelManagement.Core.Model;
using ParcelManagement.Core.Model.Configuration;
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

        public RedisConnectionFactory(IOptions<RedisSettings> options)
        {
            _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options.Value.ConnectionString));
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