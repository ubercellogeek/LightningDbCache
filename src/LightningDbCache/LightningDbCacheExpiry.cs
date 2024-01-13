using Microsoft.Extensions.Caching.Distributed;

namespace LightningDbCache
{
    public class LightningDbCacheExpiry
    {
        private const long _notSet = -1;
        public long AbsoluteExpirationUtcTicks { get; private set; } = _notSet;
        public long SlidingExpirationOffsetTicks { get; private set; } = _notSet;
        public long LastAccessedUtcTicks { get; private set; } = _notSet;
        public bool Expired { get => HasExpired(); }

        public static implicit operator byte[](LightningDbCacheExpiry entry) => entry.ToBytes();
        public static explicit operator LightningDbCacheExpiry(byte[] bytes) => new(bytes);

        public LightningDbCacheExpiry()
        {

        }

        internal LightningDbCacheExpiry(DistributedCacheEntryOptions options)
        {
            var utcNowTicks = DateTimeOffset.Now.UtcTicks;

            if (options.AbsoluteExpiration != null && options.AbsoluteExpirationRelativeToNow != null)
            {
                AbsoluteExpirationUtcTicks =
                    options.AbsoluteExpiration.Value.UtcTicks < (options.AbsoluteExpirationRelativeToNow.Value.Ticks + utcNowTicks) ?
                        options.AbsoluteExpiration.Value.UtcTicks : (options.AbsoluteExpirationRelativeToNow.Value.Ticks + utcNowTicks);
            }

            // Set from AbsoluteExpiration if available
            AbsoluteExpirationUtcTicks = AbsoluteExpirationUtcTicks == _notSet && options.AbsoluteExpiration != null ? options.AbsoluteExpiration.Value.UtcTicks : AbsoluteExpirationUtcTicks;

            // Set from AbsoluteExpirationRelativeToNow if available
            AbsoluteExpirationUtcTicks = AbsoluteExpirationUtcTicks == _notSet && options.AbsoluteExpirationRelativeToNow != null ? (options.AbsoluteExpirationRelativeToNow.Value.Ticks + utcNowTicks) : AbsoluteExpirationUtcTicks;

            // Sliding expiration
            SlidingExpirationOffsetTicks = options.SlidingExpiration != null ? options.SlidingExpiration.Value.Ticks : _notSet;

            UpdateLastAccessed(utcNowTicks);
        }

        internal LightningDbCacheExpiry(byte[] bytes)
        {
            if (bytes.Length != 24)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "Payload must be exactly 24 bytes long.");
            }

            var spanBytes = new Span<byte>(bytes);
            LastAccessedUtcTicks = BitConverter.ToInt64(spanBytes[..8]);
            SlidingExpirationOffsetTicks = BitConverter.ToInt64(spanBytes[8..16]);
            AbsoluteExpirationUtcTicks = BitConverter.ToInt64(spanBytes[16..]);  
        }

        internal void UpdateLastAccessed()
        {
            UpdateLastAccessed(DateTimeOffset.Now.UtcTicks);
        }

        private void UpdateLastAccessed(long utcTicks)
        {
            LastAccessedUtcTicks = utcTicks;
        }

        private bool HasExpired()
        {
            var utcNowTicks = DateTimeOffset.Now.UtcTicks;

            if(AbsoluteExpirationUtcTicks != _notSet)
            {
                return utcNowTicks > AbsoluteExpirationUtcTicks;
            }

            if(SlidingExpirationOffsetTicks != _notSet)
            {
                return utcNowTicks > (LastAccessedUtcTicks + SlidingExpirationOffsetTicks);
            }

            return false;
        }

        private byte[] ToBytes()
        {
            var result = new byte[24];

            Buffer.BlockCopy(BitConverter.GetBytes(LastAccessedUtcTicks), 0, result, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(SlidingExpirationOffsetTicks), 0, result, 8, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(AbsoluteExpirationUtcTicks), 0, result, 16, 8);

            return result;
        }
    }
}