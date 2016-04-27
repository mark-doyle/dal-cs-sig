using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Constants
{
    public static class ConfigurationKeys
    {
        public struct ConnectionStrings
        {
            public const string Redis = "RedisConnectionString";
            public const string AzureSearch = "AzureSearchConnectionString";
            public const string AzureStorage = "AzureStorageConnectionString";

        }

    }
}
