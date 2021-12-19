using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoteApp.Cache
{
    public class RedisStorage<TItem> : IStorage<TItem> where TItem : IStorageId
    {
        private readonly IRedisConnectionFactory _factory;

        public RedisStorage(IRedisConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<TItem> GetOrSetProbabilisticItem(string key, Func<Task<TItem>> func, TimeSpan timeExpire, int beta = 1)
        {
            var redis = await _factory.ConnectAsync();
            var database = redis.GetDatabase();

            var stringResultWithExpiry = await database.StringGetWithExpiryAsync(new RedisKey(BuildKey(key)));

            if (stringResultWithExpiry.Value.HasValue)
                return JsonConvert.DeserializeObject<TItem>(stringResultWithExpiry.Value);

            var deltaInSeconds = TimeSpan.FromSeconds(60);

            var keyTimeExpire = stringResultWithExpiry.Expiry ?? TimeSpan.FromSeconds(300);
            var subtractPeriod = deltaInSeconds * beta * Math.Log(new Random().NextDouble());

            if (!stringResultWithExpiry.Value.HasValue ||
                DateTime.Now.Subtract(subtractPeriod).Ticks >= keyTimeExpire.Ticks)
            {
                var start = DateTime.Now;

                var result = await func();
                var serializedStringObj = JsonConvert.SerializeObject(result);

                deltaInSeconds = DateTime.Now - start;

                await database.StringSetAsync(BuildKey(key),
                    new RedisValue(JsonConvert.SerializeObject(serializedStringObj + $"[delta]:[{deltaInSeconds}]")), timeExpire);
            }

            return default;
        }

        public async Task<TItem> GetItem(string key)
        {
            var redis = await _factory.ConnectAsync();
            var database = redis.GetDatabase();

            var stringResult = await database.StringGetAsync(BuildKey(key));
            return JsonConvert.DeserializeObject<TItem>(stringResult);
        }

        public async Task<TItem> GetOrSetItem(string key, Func<Task<TItem>> func)
        {
            var redis = await _factory.ConnectAsync();
            var database = redis.GetDatabase();

            var stringResult = await database.StringGetAsync(new RedisKey(BuildKey(key)));

            if (stringResult.HasValue)
                return JsonConvert.DeserializeObject<TItem>(stringResult);

            var result = await func();

            var serializedStringObj = JsonConvert.SerializeObject(result);
            await database.StringSetAsync(BuildKey(key),
                new RedisValue(JsonConvert.SerializeObject(serializedStringObj)),
                TimeSpan.FromMinutes(15));

            return result;
        }

        public async Task SetItem(string key, TItem item)
        {
            var redis = await _factory.ConnectAsync();
            var database = redis.GetDatabase();

            var serializedStringObj = JsonConvert.SerializeObject(item);
            await database.StringSetAsync(BuildKey(key),
                new RedisValue(serializedStringObj),
                TimeSpan.FromMinutes(15));
        }

        public async Task<IEnumerable<TItem>> GetBatch(IEnumerable<string> keys)
        {
            var redis = await _factory.ConnectAsync();
            var batch = redis.GetDatabase().CreateBatch();

            var testTasks = batch.StringGetAsync(keys.Select(x => new RedisKey(BuildKey(x))).ToArray());
            var result1 = await Task.WhenAll(testTasks);

            var getRedisBatchTasks = keys.Select(x => batch.StringGetAsync(new RedisKey(BuildKey(x))));

            batch.Execute();

            var result = await Task.WhenAll(getRedisBatchTasks);

            return result.Select(x => JsonConvert.DeserializeObject<TItem>(x));
        }

        /// <summary>
        /// Example of redis batch
        /// Better to use Pipeline
        /// https://stackoverflow.com/questions/27796054/pipelining-vs-batching-in-stackexchange-redis
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public async Task SetBatch(IEnumerable<TItem> items)
        {
            var redis = await _factory.ConnectAsync();
            var batch = redis.GetDatabase().CreateBatch();

            var setRedisBatchTasks = items
                .Select(x => batch.StringSetAsync(
                    new RedisKey(BuildKey(x.Id)), new RedisValue(JsonConvert.SerializeObject(x)), TimeSpan.FromMinutes(15)));

            batch.Execute();

            await Task.WhenAll(setRedisBatchTasks);
        }

        /// <summary>
        /// Example of redis pipelining
        /// https://stackoverflow.com/questions/27796054/pipelining-vs-batching-in-stackexchange-redis
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public async Task SetPipeline(IEnumerable<TItem> items)
        {
            var redis = await _factory.ConnectAsync();
            var redisDb = redis.GetDatabase();

            var setRedisBatchTasks = items
                .Select(x => redisDb.StringSetAsync(
                    new RedisKey(BuildKey(x.Id)), new RedisValue(JsonConvert.SerializeObject(x)), TimeSpan.FromMinutes(15)));

            await Task.WhenAll(setRedisBatchTasks);
        }

        /// <summary>
        /// Example of redis pipelining
        /// https://stackoverflow.com/questions/27796054/pipelining-vs-batching-in-stackexchange-redis
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public async Task SetPipelineWithFireAndForget(IEnumerable<TItem> items)
        {
            var redis = await _factory.ConnectAsync();
            var redisDb = redis.GetDatabase();

            var setRedisBatchTasks = items
                .Select(x => redisDb.StringSetAsync(
                    new RedisKey(BuildKey(x.Id)), new RedisValue(JsonConvert.SerializeObject(x)),
                    TimeSpan.FromMinutes(15), flags: CommandFlags.FireAndForget));

            await Task.WhenAll(setRedisBatchTasks);
        }

        public async Task PingPipelineAsync(int countOfPing)
        {
            var redis = await _factory.ConnectAsync();
            var redisDb = redis.GetDatabase();

            for (int i = 0; i < countOfPing; i++)
            {
                await redisDb.PingAsync();
            }
        }

        public async Task PingAsync(int countOfPing)
        {
            var redis = await _factory.ConnectAsync();
            var redisDb = redis.GetDatabase();

            var pingRedisTasks = new List<Task>();
            for (int i = 0; i < countOfPing; i++)
            {
                pingRedisTasks.Add(redisDb.PingAsync());
            }

            await Task.WhenAll(pingRedisTasks);
        }

        public string BuildKey(string key) => $"{typeof(TItem).Name}_{key}";

        public async Task<IEnumerable<RedisKey>> GetAllKeys()
        {
            var redis = await _factory.ConnectAsync();
            var server = redis.GetServer(_factory.GetConnectionString());
            return server.Keys();
        }

        public async Task<IEnumerable<TItem>> GetAllItems()
        {
            var allItems = new List<TItem>();
            var keys = await GetAllKeys();

            var redis = await _factory.ConnectAsync();
            var database = redis.GetDatabase();

            foreach (var item in keys)
            {
                var stringResult = await database.StringGetAsync(item);
                TItem resultItem = JsonConvert.DeserializeObject<TItem>(stringResult.ToString());
                allItems.Add(resultItem);
            }

            return allItems;
        }
    }
}
