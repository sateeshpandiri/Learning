using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.ApplicationServer.Caching;
using System.Runtime.Caching;

namespace AppfabricMonitoring
{
    class Program
    {
        private static DataCacheFactory _factory;
        private static DataCache _cache;
        static void Main(string[] args)
        {
            
            List<DataCacheServerEndpoint> servers = new List<DataCacheServerEndpoint>(1);
            // Get the comma dilimited values for all of the different servers for cache.
            string[] serverNames = ConfigurationManager.AppSettings["CacheServerNames"].ToString().Split(',');

            // Iterate through the serverNames and add the servers
            foreach (string serverName in serverNames)
            {
                servers.Add(new DataCacheServerEndpoint(serverName, 22233));
            }

            // Create cache configuration
            DataCacheFactoryConfiguration configuration = new DataCacheFactoryConfiguration();

            // Changed the DataCacheSecurityMode to transport....
            var security = new DataCacheSecurity(DataCacheSecurityMode.Transport, DataCacheProtectionLevel.None);
            configuration.SecurityProperties = security;

            // Set the cache host
            configuration.Servers = servers;

            // Set default properties for local cache
            configuration.LocalCacheProperties = new DataCacheLocalCacheProperties(
                Convert.ToInt32(ConfigurationManager.AppSettings["LocalCacheMaxObjectCount"]),
                TimeSpan.FromSeconds(System.Convert.ToInt32(ConfigurationManager.AppSettings["CacheInitTimeoutSeconds"].ToString())),
                DataCacheLocalCacheInvalidationPolicy.TimeoutBased
                );

            //configuration.SecurityProperties = new DataCacheSecurity();
            // Disable tracing
            DataCacheClientLogManager.ChangeLogLevel(System.Diagnostics.TraceLevel.Off);

            // Trying to set the timeout
            configuration.RequestTimeout = TimeSpan.FromSeconds(20);


            // LoggingService.WriteTrace(TraceSeverity.Verbose, LoggingService.PPQRCategory.PPQRCache, "CacheUtil: In the try catch block for cache initialization");
            // Pass configuration settings to cacheFactory constructor
            _factory = new DataCacheFactory(configuration);

            // LoggingService.WriteTrace(TraceSeverity.Verbose, LoggingService.PPQRCategory.PPQRCache, "CacheUtil: Was able to get the factory");


            // Get reference to named cache called "default"
            _cache = _factory.GetCache("default");

            int totalItemCount = 0;
            int oldtotalItemCount = 0;
            foreach (string regionName in _cache.GetSystemRegions())
            {
                oldtotalItemCount = totalItemCount;
                totalItemCount += _cache.GetObjectsInRegion(regionName).Count();
                if (totalItemCount > oldtotalItemCount)
                {
                        foreach (var keyvaluepair in _cache.GetObjectsInRegion(regionName))
                        {
                            var result = string.Format(
                                    "Key: {0}. Value: {1}" +"\n", keyvaluepair.Key, keyvaluepair.Value);
                            Console.WriteLine(result);
                        }
                }
            }
            
            System.Console.WriteLine("Toatal Item Count"+totalItemCount.ToString());
        }
    }
}

