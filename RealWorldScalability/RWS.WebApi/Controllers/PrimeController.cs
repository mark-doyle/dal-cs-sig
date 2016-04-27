﻿using RWS.Lib.Caching;
using RWS.Lib.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RWS.WebApi.Controllers
{
    public class PrimeController : ApiController
    {
        #region Calculation only. No storage, no cache

        [HttpGet]
        [ActionName("FindPrimesInRange")]
        public List<int> FindPrimesInRange(int start, int end)
        {
            List<int> primes = new List<int>();

            for (int test = start; test <= end; test++)
            {
                if (PrimaryNumberCalculator.IsPrime(test))
                    primes.Add(test);
            }

            return primes;
        }

        [HttpGet]
        [ActionName("FindPrimesInRangeAsync")]
        public async Task<List<int>> FindPrimesInRangeAsync(int start, int end)
        {
            return await Task.Run(() => FindPrimesInRange(start, end));
        }

        #endregion // Calculation only. No storage, no cache

        #region Calculation with in-memory cache, no storage

        [HttpGet]
        [ActionName("FindCachedPrimesInRange")]
        public List<int> FindCachedPrimesInRange(int start, int end)
        {
            string key = "FindCachedPrimesInRange-" + start.ToString() + "-" + end.ToString();
            return InMemoryCacheService.GetCachedItem<List<int>>(key, () =>
            {
                List<int> primes = new List<int>();

                for (int test = start; test <= end; test++)
                {
                    if (PrimaryNumberCalculator.IsPrime(test))
                        primes.Add(test);
                }

                return primes;
            });

        }

        [HttpGet]
        [ActionName("FindCachedPrimesInRangeAsync")]
        public async Task<List<int>> FindCachedPrimesInRangeAsync(int start, int end)
        {
            string key = "FindCachedPrimesInRangeAsync-" + start.ToString() + "-" + end.ToString();
            return await InMemoryCacheService.GetCachedItemAsync<List<int>>(key, async () =>
            {
                return await Task.Run(() => FindPrimesInRange(start, end));
            });
        }

        #endregion // Calculation with in-memory cache, no storage

    }
}
