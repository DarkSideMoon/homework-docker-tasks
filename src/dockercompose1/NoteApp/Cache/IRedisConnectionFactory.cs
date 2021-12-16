using StackExchange.Redis;
using System.Threading.Tasks;

namespace NoteApp.Cache
{
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer Connect();

        Task<ConnectionMultiplexer> ConnectAsync();
    }
}
