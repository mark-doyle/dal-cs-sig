using Newtonsoft.Json;
using RWS.Lib.Configuration;
using RWS.Lib.Constants;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Caching
{
    public static class DistributedCacheService
    {
        #region Declarations

        private static ConnectionMultiplexer _MULTIPLEXER;
        private static object __LOCK = new object();

        private static ConnectionMultiplexer RedisInstance
        {
            get
            {
                if (_MULTIPLEXER == null)
                {
                    lock (__LOCK)
                    {
                        _MULTIPLEXER = ConnectionMultiplexer.Connect(ConfigurationReader.GetInstance().GetStringProperty(ConfigurationKeys.ConnectionStrings.Redis));
                    }

                }
                return _MULTIPLEXER;
            }
        }
        private static readonly JsonSerializerSettings JSON_SERIALIZER_SETTINGS = new JsonSerializerSettings()
        {
            MaxDepth = 10
        };

        #endregion // Declarations

        #region Private Methods

        private static IDatabase GetCacheInstance(int index = 0)
        {
            IDatabase cache = RedisInstance.GetDatabase(index);
            return cache;
        }

        #endregion // Private Methods

        #region Public Static Methods

        public static async Task<T> GetCachedItemAsync<T>(string key, Func<Task<T>> fallback = null, double fallbackMinutesToCache = 10, int databaseIndex = 0) where T : class
        {
            T instance = null;
            IDatabase cache = GetCacheInstance(databaseIndex);

            if (cache != null)
            {
                //var cached = cache.StringGet(key);
                var cached = await cache.StringGetAsync(key);
                if (cached.HasValue && !cached.IsNullOrEmpty)
                {
                    instance = await Task.Run(() => JsonConvert.DeserializeObject<T>(cached, JSON_SERIALIZER_SETTINGS));
                }
            }

            if (instance == null && fallback != null)
            {
                try
                {
                    instance = await fallback();
                    await AddItemAsync(instance, key, fallbackMinutesToCache);
                }
                catch 
                {
                    instance = null;
                }
            }
            return instance;
        }

        public static T GetCachedItem<T>(string key, Func<T> fallback = null, double fallbackMinutesToCache = 10, int databaseIndex = 0) where T : class
        {
            T instance = null;
            IDatabase cache = GetCacheInstance(databaseIndex);

            if (cache != null)
            {
                var cached = cache.StringGet(key);
                if (cached.HasValue && !cached.IsNullOrEmpty)
                {
                    instance = JsonConvert.DeserializeObject<T>(cached, JSON_SERIALIZER_SETTINGS);
                }
            }

            if (instance == null && fallback != null)
            {
                try
                {
                    instance = fallback();
                    AddItem(instance, key, fallbackMinutesToCache);
                }
                catch
                {
                    instance = null;
                }
            }
            return instance;
        }

        public static async Task AddItemAsync(object objectToCache, string key, double minutesToCache = 10, int databaseIndex = 0)
        {
            if (objectToCache != null)
            {
                string json = JsonConvert.SerializeObject(objectToCache, JSON_SERIALIZER_SETTINGS);
                IDatabase cache = GetCacheInstance(databaseIndex);
                if (cache != null)
                {
                    await cache.StringSetAsync(key, json, TimeSpan.FromMinutes(minutesToCache), When.Always);
                }
            }
        }

        public static void AddItem(object objectToCache, string key, double minutesToCache = 10, int databaseIndex = 0)
        {
            if (objectToCache != null)
            {
                string json = JsonConvert.SerializeObject(objectToCache, JSON_SERIALIZER_SETTINGS);
                IDatabase cache = GetCacheInstance(databaseIndex);
                if (cache != null)
                {
                    cache.StringSet(key, json, TimeSpan.FromMinutes(minutesToCache), When.Always);
                }
            }
        }

        #endregion // Public Static Methods

    }
}
