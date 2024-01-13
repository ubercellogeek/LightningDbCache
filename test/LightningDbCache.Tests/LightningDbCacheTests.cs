using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LightningDbCache.Tests
{
    public class LightningDbCacheTests
    {
        private static ServiceProvider _provider;
        private const string _testDataFolder = "test-data";
        static LightningDbCacheTests()
        {
            var dir = Directory.GetCurrentDirectory();
            var configBuilder = new ConfigurationBuilder();
            var basePath = _testDataFolder;
            var expirationScanFrequency = 1;
            var configuration = configBuilder.Build();
            var services = new ServiceCollection();

            services.UseLightningDbCache((opts) => {
                opts.BasePath = basePath;
                opts.ExpirationScanFrequency = TimeSpan.FromSeconds(expirationScanFrequency);
            });

            _provider = services.BuildServiceProvider();
            
        }

        [Fact]
        public void Should_Dispose()
        {
            // Arrange
            var cache = BuildInstance();

            // Act
            cache.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => cache.Get("test"));
        }

        [Fact]
        public void Should_Not_Allow_Keys_That_Are_Too_Long()
        {
            // Arrange
            var key = new string('A',512);
            var cache = _provider.GetRequiredService<IDistributedCache>();

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => cache.Set(key, Array.Empty<byte>()));
        }

        [Fact]
        public async Task Should_Not_Allow_Keys_That_Are_Too_Long_Async()
        {
            // Arrange
            var key = new string('A',512);
            var cache = _provider.GetRequiredService<IDistributedCache>();

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => cache.SetAsync(key, Array.Empty<byte>()));
        }

        [Fact]
        public void Should_Not_Allow_Null_Keys_On_Set()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            string key = default!;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => cache.Set(key, Array.Empty<byte>()));
        }

        [Fact]
        public void Should_Not_Allow_Null_Keys_On_Get()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            string key = default!;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => cache.Get(key));
        }

        [Fact]
        public async Task Should_Not_Allow_Null_Keys_On_SetAsync()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            string key = default!;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.SetAsync(key, Array.Empty<byte>()));
        }

        [Fact]
        public async Task Should_Not_Allow_Null_Keys_On_GetAsync()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            string key = default!;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.GetAsync(key));
        }

        [Fact]
        public void Should_Not_Allow_Invalid_Map_Size_Configuration()
        {
            // Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => new LightningDbCache(new LightningDbCacheOptions() { MapSize = 0 }, NullLoggerFactory.Instance));
        }

        [Fact]
        public async Task Should_Get()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            var key = "get";
            var value = "test-value";
            var valueBytes = Encoding.UTF8.GetBytes(value);

            // Act
            await cache.SetAsync(key, valueBytes);
            var result = await cache.GetAsync(key);

            // Assert
            Assert.Equal(valueBytes, result);
        }

        [Fact]
        public async Task Should_Get_Within_Expiry()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            var key = "get-within-expiry";
            var value = "test-value";
            var valueBytes = Encoding.UTF8.GetBytes(value);

            // Act
            await cache.SetAsync(key, valueBytes, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            var result = await cache.GetAsync(key);

            // Assert
            Assert.Equal(valueBytes, result);
        }

        [Fact]
        public async Task Should_Not_Get_Outside_Expiry()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            var key = "get-outside-expiry";
            var value = "test-value";
            var valueBytes = Encoding.UTF8.GetBytes(value);

            // Act
            await cache.SetAsync(key, valueBytes, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromTicks(1) });
            await Task.Delay(10);
            var result = await cache.GetAsync(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Should_Not_Get_Non_Existant()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            var key = "get-non-existant";

            // Act
            var result = await cache.GetAsync(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Should_Refresh_By_Key()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            var key = "refresh-key";
            var value = "test-value";
            var valueBytes = Encoding.UTF8.GetBytes(value);

            // Act
            await cache.SetAsync(key, valueBytes, new DistributedCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMilliseconds(3) });

            var result = await cache.GetAsync(key);

            // Assert
            Assert.Equal(valueBytes, result);
        }

        [Fact]
        public async Task Should_Delete_By_Key()
        {
            // Arrange
            var cache = _provider.GetRequiredService<IDistributedCache>();
            var key = "delete-key";
            var value = "test-value";
            var valueBytes = Encoding.UTF8.GetBytes(value);

            // Act
            await cache.SetAsync(key, valueBytes);
            var result = await cache.GetAsync(key);
            
            await cache.RemoveAsync(key);
            var deletedResult = await cache.GetAsync(key);

            // Assert
            Assert.NotNull(result);
            Assert.Null(deletedResult);
        }

        public LightningDbCache BuildInstance()
        {
             // Arrange
            var configBuilder = new ConfigurationBuilder();
            var mapSize = 18000;
            var basePath = "test";
            var expirationScanFrequency = 1;
            var configuration = configBuilder.Build();
            var services = new ServiceCollection();

            services.UseLightningDbCache((opts) => {
                opts.BasePath = basePath;
                opts.MapSize = mapSize;
                opts.ExpirationScanFrequency = TimeSpan.FromSeconds(expirationScanFrequency);
            });

            var provider = services.BuildServiceProvider();

            // Act
            var opts = provider.GetService<IOptions<LightningDbCacheOptions>>();
            var cache = provider.GetRequiredService<IDistributedCache>();

            return (LightningDbCache)cache;
        }
    }
}