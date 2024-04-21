using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LightningDbCache.Tests
{
    public class LightningDbCacheServiceExtensionTests
    {
        
        [Fact]
        public void Should_Resolve_Options_For_Simple_Registration()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configBuilder.Build());
            services.UseLightningDbCache();
            var provider = services.BuildServiceProvider();
            
            // Act
            var config = provider.GetService<IOptions<LightningDbCacheOptions>>();
            var cache = provider.GetService<IDistributedCache>();

            // Assert  
            Assert.IsType<LightningDbCache>(cache);
            Assert.NotNull(config);
        }


        [Fact]
        public void Should_Resolve_Options_For_Named_Configuration_Registration()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder();
            var maxSize = 18000;
            var dataPath = "test";
            var expirationScanFrequency = 20;

            var configValues = new Dictionary<string, string?>
            {
                {"MyCustomOptions:MaxSize", maxSize.ToString()},
                {"MyCustomOptions:DataPath", dataPath},
                {"MyCustomOptions:ExpirationScanFrequency", $"00:00:{expirationScanFrequency}"}
            };

            configBuilder.AddInMemoryCollection(configValues);

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configBuilder.Build());
            services.UseLightningDbCache("MyCustomOptions");

            var provider = services.BuildServiceProvider();

            // Act
            var opts = provider.GetService<IOptions<LightningDbCacheOptions>>();
            var cache = provider.GetService<IDistributedCache>();
            
            // Assert
            Assert.NotNull(opts);
            Assert.Equal(maxSize, opts.Value.MaxSize);
            Assert.Equal(dataPath, opts.Value.DataPath);
            Assert.Equal(expirationScanFrequency, opts.Value.ExpirationScanFrequency.TotalSeconds);
        }

        [Fact]
        public void Should_Resolve_Options_For_Named_ConfigurationSection_Registration()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder();
            var maxSize = 18000;
            var dataPath = "test";
            var expirationScanFrequency = 20;

            var configValues = new Dictionary<string, string?>
            {
                {"MyCustomOptions:MaxSize", maxSize.ToString()},
                {"MyCustomOptions:DataPath", dataPath},
                {"MyCustomOptions:ExpirationScanFrequency", $"00:00:{expirationScanFrequency}"}
            };

            configBuilder.AddInMemoryCollection(configValues);
            var configuration = configBuilder.Build();
            var services = new ServiceCollection();

            services.UseLightningDbCache(configuration.GetSection("MyCustomOptions"));

            var provider = services.BuildServiceProvider();

            // Act
            var opts = provider.GetService<IOptions<LightningDbCacheOptions>>();
            var cache = provider.GetService<IDistributedCache>();
            
            // Assert
            Assert.NotNull(opts);
            Assert.Equal(maxSize, opts.Value.MaxSize);
            Assert.Equal(dataPath, opts.Value.DataPath);
            Assert.Equal(expirationScanFrequency, opts.Value.ExpirationScanFrequency.TotalSeconds);
        }

        [Fact]
        public void Should_Resolve_Options_For_Configuration_Action_Registration()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder();
            var maxSize = 18000;
            var dataPath = "test";
            var expirationScanFrequency = 20;
            var configuration = configBuilder.Build();
            var services = new ServiceCollection();

            services.UseLightningDbCache((opts) => {
                opts.DataPath = dataPath;
                opts.MaxSize = maxSize;
                opts.ExpirationScanFrequency = TimeSpan.FromSeconds(expirationScanFrequency);
            });

            var provider = services.BuildServiceProvider();

            // Act
            var opts = provider.GetService<IOptions<LightningDbCacheOptions>>();
            var cache = provider.GetService<IDistributedCache>();
            
            // Assert
            Assert.NotNull(opts);
            Assert.Equal(maxSize, opts.Value.MaxSize);
            Assert.Equal(dataPath, opts.Value.DataPath);
            Assert.Equal(expirationScanFrequency, opts.Value.ExpirationScanFrequency.TotalSeconds);
        }
    }
}