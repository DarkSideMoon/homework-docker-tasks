using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoteApp.Cache
{
    public interface IStorage<TItem> where TItem : IStorageId
    {
        /// <summary>
        /// Probabilistic early expiration
        /// https://en.wikipedia.org/wiki/Cache_stampede
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="timeExpire"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        Task<TItem> GetOrSetProbabilisticItem(string key, Func<Task<TItem>> func, TimeSpan timeExpire, int beta = 1);

        Task<TItem> GetItem(string key);

        Task<TItem> GetOrSetItem(string key, Func<Task<TItem>> func);

        Task SetItem(string key, TItem item);

        Task SetBatch(IEnumerable<TItem> items);

        Task SetPipeline(IEnumerable<TItem> items);

        Task SetPipelineWithFireAndForget(IEnumerable<TItem> items);

        Task<IEnumerable<TItem>> GetBatch(IEnumerable<string> keys);

        Task<IEnumerable<RedisKey>> GetAllKeys();

        Task<IEnumerable<TItem>> GetAllItems();

        Task PingPipelineAsync(int countOfPing);

        Task PingAsync(int countOfPing);

        string BuildKey(string key);
    }
}
