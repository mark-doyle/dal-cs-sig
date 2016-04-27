using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Caching
{
    public static class InMemoryCacheService
    {
        private static readonly ObjectCache __CACHE = MemoryCache.Default;

        public static void AddItem(object objectToCache, string key, double minutesToCache = 10, int databaseIndex = 0)
        {
            if (objectToCache != null)
                __CACHE.Add(key, objectToCache, DateTime.UtcNow.AddMinutes(minutesToCache));
        }

        public static async Task AddItemAsync(object objectToCache, string key, double minutesToCache = 10, int databaseIndex = 0)
        {
            await Task.Run(() => AddItem(objectToCache, key, minutesToCache, databaseIndex));
        }

        public static void DeleteItem(string key, int databaseIndex = 0)
        {
            if (__CACHE.Contains(key))
                __CACHE.Remove(key);
        }

        public static async Task DeleteItemAsync(string key, int databaseIndex = 0)
        {
            await Task.Run(() => DeleteItem(key));
        }

        public static T GetCachedItem<T>(string key, Func<T> fallback = null, double fallbackMinutesToCache = 10, int databaseIndex = 0) where T : class
        {
            try
            {
                T result = __CACHE[key] as T;
                if (result == null)
                {
                    result = fallback();
                    AddItem(result, key, fallbackMinutesToCache, databaseIndex);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<T> GetCachedItemAsync<T>(string key, Func<Task<T>> fallback = null, double fallbackMinutesToCache = 10, int databaseIndex = 0) where T : class
        {
            try
            {
                T result = __CACHE[key] as T;
                if (result == null)
                {
                    result = await fallback();
                    await AddItemAsync(result, key, fallbackMinutesToCache, databaseIndex);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

    }
}
