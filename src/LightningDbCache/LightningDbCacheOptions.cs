using Microsoft.Extensions.Options;
using LightningDB;

namespace LightningDbCache
{
    /// <summary>
    /// Represents the various configuration options for the underlying LightningDB cache.
    /// </summary>
    public class LightningDbCacheOptions : IOptions<LightningDbCacheOptions>
    {
        private readonly EnvironmentConfiguration _environmentConfiguration;
        private const long _defaultMapSize = 1024 * 1024 * 200; // 200MB

        /// <summary>
        /// Initializes a new instance of the <see cref="LightningDbCacheOptions"/> class.
        /// </summary>
        public LightningDbCacheOptions()
        {
            _environmentConfiguration = new EnvironmentConfiguration()
            {
                MaxDatabases = 2,
                MaxReaders = 2048,
                MapSize = _defaultMapSize
            };
        }

        /// <summary>
        /// The path to store the cache database files.
        /// </summary>
        public string? DataPath { get; set; }

        /// <summary>
        /// The maximum size of the cache database. The default is 209715200 bytes (200MB). This should be set to a multiple of the underlying OS page size (usually 4096). 
        /// NOTE: Reducing this size after data has been written to the database may result in data loss. Change the value of this value to meet your needs.
        /// </summary>
        public long MaxSize
        {
            get
            {
                return _environmentConfiguration.MapSize;
            }
            set
            {
                _environmentConfiguration.MapSize = value;
            }
        }

        internal EnvironmentConfiguration EnvironmentConfiguration => _environmentConfiguration;

        /// <summary>
        /// Gets or sets the minimum length of time between successive scans for expired items.
        /// </summary>
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets the value of the options.
        /// </summary>
        public LightningDbCacheOptions Value => this;
    }
}
