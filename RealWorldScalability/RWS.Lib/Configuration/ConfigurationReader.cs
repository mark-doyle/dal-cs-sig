using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Configuration
{
    public class ConfigurationReader
    {
        // Singleton instance
        private static volatile ConfigurationReader _singletonInstance;

        /// <summary>
        /// Private constructor for singleton pattern
        /// </summary>
        private ConfigurationReader()
        {
        }

        /// <summary>
        /// Accessor for the singleton instance. Creates the instance if it
        /// doesn't already exist.
        /// </summary>
        /// <returns></returns>
        public static ConfigurationReader GetInstance()
        {
            // Create the instance if it doesn't exist yet
            return _singletonInstance ?? (_singletonInstance = new ConfigurationReader());
        }

        public string GetStringProperty(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("Null key cannot be referenced in configuration file.");
            }

            return CloudConfigurationManager.GetSetting(key);
        }

        /// <summary>
        /// Attempts to parse the specified value into a boolean before returning.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetBooleanProperty(string key)
        {
            return bool.Parse(GetStringProperty(key));
        }

        /// <summary>
        /// Attempts to parse the specified value into an int before returning.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetIntProperty(string key)
        {
            return int.Parse(GetStringProperty(key));
        }

        /// <summary>
        /// Attempts to parse the specified value into a double before returning.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double GetDoubleProperty(string key)
        {
            return double.Parse(GetStringProperty(key));
        }

        /// <summary>
        /// Attempts to parse the specified value into a Guid before returning.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Guid GetGuidProperty(string key)
        {
            return Guid.Parse(GetStringProperty(key));
        }
    }
}
