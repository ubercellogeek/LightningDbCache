using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LightningDbCache
{
    /// <summary>
    /// Extension methods for registering the <see cref="LightningDbCache"/> with an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a singleton <see cref="IDistributedCache"/> instance using the <see cref="LightningDbCache"/> implementation with the default configuration.
        /// </summary>
        /// <param name="services">The service collection to register against.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddLightningDbCache(this IServiceCollection services)
        {
            services.AddLightningDbCache(nameof(LightningDbCacheOptions));
            return services;
        }

        /// <summary>
        /// Registers a singleton <see cref="IDistributedCache"/> instance using the <see cref="LightningDbCache"/> implementation with the specified configuration.
        /// </summary>
        /// <param name="services">The service collection to register against.</param>
        /// <param name="namedConfigurationSection">The <see cref="IConfigurationSection"/> that contains the <see cref="LightningDbCacheOptions"/> to use as the configuration for the cache.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddLightningDbCache(this IServiceCollection services, IConfigurationSection namedConfigurationSection)
        {
            services.AddLogging();
            services.AddSingleton<IDistributedCache, LightningDbCache>();
            services.Configure<LightningDbCacheOptions>(namedConfigurationSection);
            return services;
        }

        /// <summary>
        /// Registers a singleton <see cref="IDistributedCache"/> instance using the <see cref="LightningDbCache"/> implementation with the specified configuration path.
        /// </summary>
        /// <param name="services">The service collection to register against.</param>
        /// <param name="configSectionPath">The configuration section path that contains the <see cref="LightningDbCacheOptions"/> to use as the configuration for the cache.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddLightningDbCache(this IServiceCollection services, string configSectionPath)
        {
            services.AddLogging();
            services.AddSingleton<IDistributedCache, LightningDbCache>();
            services.AddOptions<LightningDbCacheOptions>()
                .BindConfiguration(configSectionPath);

            return services;
        }

        /// <summary>
        /// Registers a singleton <see cref="IDistributedCache"/> instance using the <see cref="LightningDbCache"/> implementation with the specified configuration.
        /// </summary>
        /// <param name="services">The service collection to register against.</param>
        /// <param name="setupAction">An action that provides the <see cref="LightningDbCacheOptions"/> to use as the configuration for the cache.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddLightningDbCache(this IServiceCollection services, Action<LightningDbCacheOptions> setupAction)
        {
            services.AddLogging();
            services.AddSingleton<IDistributedCache, LightningDbCache>();
            services.Configure(setupAction);
            return services;
        }
    }
}
