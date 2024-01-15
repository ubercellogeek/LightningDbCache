using Microsoft.Extensions.Options;
using LightningDB;

namespace LightningDbCache
{
    public class LightningDbCacheOptions : IOptions<LightningDbCacheOptions>
    {
        private readonly EnvironmentConfiguration _environmentConfiguration;

        public LightningDbCacheOptions()
        {
            _environmentConfiguration = new EnvironmentConfiguration()
            {
                MapSize = 200 * 1024 * 1024, // 200MB 
                MaxDatabases = 2,
                MaxReaders = 2048
            };
        }

        /// <summary>
        /// The path to store the cache database files.
        /// </summary>
        public string? DataPath { get; set; }

        /// <summary>
        /// The maximum size of the cache database.
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

        public LightningDbCacheOptions Value => this;
    }
}
