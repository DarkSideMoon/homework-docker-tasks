using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace NoteApp.Cache
{
    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly Lazy<Task<ConnectionMultiplexer>> _connection;
        private readonly IOptions<RedisSettings> _redisConnectionString;

        public RedisConnectionFactory(IOptions<RedisSettings> redis)
        {
            _connection = new Lazy<Task<ConnectionMultiplexer>>(() => ConnectionMultiplexer.ConnectAsync(redis.Value.ConnectionString));
            _redisConnectionString = redis;
        }

        public Task<ConnectionMultiplexer> ConnectAsync() => _connection.Value;

        public ConnectionMultiplexer Connect() => _connection.Value.GetAwaiter().GetResult();

        public string GetConnectionString() => _redisConnectionString.Value.ConnectionString;
    }
}
