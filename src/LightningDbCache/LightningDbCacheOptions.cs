using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace LightningDbCache
{
    public class LightningDbCacheOptions : IOptions<LightningDbCacheOptions>
    {
        public long MapSize { get; set; }
        public string? BasePath { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of time between successive scans for expired items.
        /// </summary>
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1);

        public LightningDbCacheOptions Value => this;
    }
}