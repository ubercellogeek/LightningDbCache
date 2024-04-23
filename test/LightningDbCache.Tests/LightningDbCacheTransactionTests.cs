using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using LightningDB;

namespace LightningDbCache.Tests
{
    public class LightningDbCacheTransactionTests
    {
        private static ServiceProvider _provider;
        private const string _testDataFolder = "test-tx-data";

        static LightningDbCacheTransactionTests()
        {
            var dir = Directory.GetCurrentDirectory();
            var configBuilder = new ConfigurationBuilder();
            var basePath = _testDataFolder;
            var expirationScanFrequency = 1;
            var configuration = configBuilder.Build();
            var services = new ServiceCollection();

            services.UseLightningDbCache((opts) =>
            {
                opts.DataPath = basePath;
                opts.ExpirationScanFrequency = TimeSpan.FromSeconds(expirationScanFrequency);
                opts.MaxSize = 16400;
            });

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void Should_Not_Set_For_Full_Database()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            var key = "test-key";
            
            // Fill up the database
            for(int i = 0; i < 1000; i++)
            {
                cache.Set(i.ToString(), new byte[100]);
            }

            // Act
            cache.SetString(key, "test-value");

            // Assert
            Assert.Null(cache.GetString(key));
        }
    }
}