using LightningDB;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace LightningDbCache
{
    public class LightningDbCacheOptions : EnvironmentConfiguration, IOptions<LightningDbCacheOptions>
    {
        public LightningDbCacheOptions() : base()
        {
            // Set some reasonable defaults
            MapSize = 200 * 1024 * 1024; // 200MB 
            MaxDatabases = 2;
            MaxReaders = 2048;
        }

        /// <summary>
        /// The base path for the lightning database files.
        /// </summary>
        public string? BasePath { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of time between successive scans for expired items.
        /// </summary>
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromSeconds(10);

        public LightningDbCacheOptions Value => this;

    }
}