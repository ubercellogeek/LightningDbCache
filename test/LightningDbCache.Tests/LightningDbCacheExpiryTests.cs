using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace LightningDbCache.Tests
{
    public class LightningDbCacheExpiryTests
    {

        [Fact]
        public async Task Should_Expire_Sliding_Expiration()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMilliseconds(1)
            };


            // Act
            var expiry = new LightningDbCacheExpiry(options);
            await Task.Delay(2);

            // Assert
            Assert.True(expiry.Expired);
        }

        [Fact]
        public void Should_Expire_Absolute_Expiration()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(-10)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);

            // Assert
            Assert.True(expiry.Expired);
        }


        [Fact]
        public async Task Should_Expire_Absolute_Relative_To_Now()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMicroseconds(1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);
            await Task.Delay(2);
            
            // Assert
            Assert.True(expiry.Expired);
        }

        [Fact]
        public void Should_Expire_Lesser_Absolute_Sliding_Expiration()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(-1),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);

            // Assert
            Assert.True(expiry.Expired);
        }

        [Fact]
        public async Task Should_Expire_Lesser_Absolute_Relative_To_Now_Sliding_Expiration()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMicroseconds(1),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);
            await Task.Delay(1);

            // Assert
            Assert.True(expiry.Expired);
        }

        [Fact]
        public async Task Should_Expire_When_Absolute_Is_Less_Than_AbsoluteRelativeToNow()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                AbsoluteExpiration = DateTime.Now.AddMinutes(-1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);
            await Task.Delay(2);
            
            // Assert
            Assert.True(expiry.Expired);
        }


        [Fact]
        public async Task Should_Expire_When_AbsoluteRelativeToNow_Is_Less_Than_Absolute()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMicroseconds(1),
                AbsoluteExpiration = DateTime.Now.AddMinutes(2)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);
            await Task.Delay(1);
            
            // Assert
            Assert.True(expiry.Expired);
        }

        // NOT

        [Fact]
        public void Should_Not_Expire_Future_Sliding_Expiration()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);

            // Assert
            Assert.False(expiry.Expired);
        }

        [Fact]
        public void Should_Not_Expire_Future_Absolute_Expiration()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddHours(1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);

            // Assert
            Assert.False(expiry.Expired);
        }


        [Fact]
        public async Task Should_Not_Expire_Future_Absolute_Relative_To_Now()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);
            await Task.Delay(2);
            
            // Assert
            Assert.False(expiry.Expired);
        }

        [Fact]
        public void Should_Not_Expire_Lesser_Future_Absolute_Sliding_Expiration()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);

            // Assert
            Assert.False(expiry.Expired);
        }

        [Fact]
        public void Should_Not_Expire_Lesser_Future_Absolute_Relative_To_Now_Sliding_Expiration()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);

            // Assert
            Assert.False(expiry.Expired);
        }

        [Fact]
        public void Should_Not_Expire_When_Future_Absolute_Is_Less_Than_AbsoluteRelativeToNow()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                AbsoluteExpiration = DateTime.Now.AddMinutes(1)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);
            
            // Assert
            Assert.False(expiry.Expired);
        }


        [Fact]
        public void Should_Not_Expire_When_Future_AbsoluteRelativeToNow_Is_Less_Than_Absolute()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                AbsoluteExpiration = DateTime.Now.AddMinutes(2)
            };

            // Act
            var expiry = new LightningDbCacheExpiry(options);
            
            // Assert
            Assert.False(expiry.Expired);
        }

        [Fact]
        public void Should_Default_Values_With_Empty_Constructor()
        {
            // Arrange
            var expiry = new LightningDbCacheExpiry();
            
            // Act

            // Assert
            Assert.Equal(-1, expiry.AbsoluteExpirationUtcTicks);
            Assert.Equal(-1, expiry.LastAccessedUtcTicks);
            Assert.Equal(-1, expiry.SlidingExpirationOffsetTicks);
        }

        [Fact]
        public void Should_Not_Expire_With_Defaults()
        {
            // Arrange
            var expiry = new LightningDbCacheExpiry();
            
            // Act

            // Assert
            Assert.False(expiry.Expired);
        }

        [Fact]
        public void Should_Implicitly_Cast_To_Array_Of_Bytes()
        {
            // Arrange
            var expiry = new LightningDbCacheExpiry();

            // Act
            byte[] bytes = expiry;

            // Assert
            Assert.Equal(24, bytes.Length);
            Assert.Equal(-1, BitConverter.ToInt64(bytes[0..8]));
            Assert.Equal(-1, BitConverter.ToInt64(bytes[8..16]));
            Assert.Equal(-1, BitConverter.ToInt64(bytes[16..]));
        }

        [Fact]
        public void Should_Explicitly_Cast_From_Array_Of_Bytes()
        {
            // Arrange
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            };

            var expiry = new LightningDbCacheExpiry(options);
            expiry.UpdateLastAccessed();

            // Act
            byte[] bytes = expiry;

            var expiry2 = (LightningDbCacheExpiry)bytes;

            // Assert
            Assert.Equal(expiry.AbsoluteExpirationUtcTicks, expiry2.AbsoluteExpirationUtcTicks);
            Assert.Equal(expiry.LastAccessedUtcTicks, expiry2.LastAccessedUtcTicks);
            Assert.Equal(expiry.SlidingExpirationOffsetTicks, expiry2.SlidingExpirationOffsetTicks);
        }

        [Fact]
        public void Should_Not_Parse_Invalid_Byte_Array()
        {
            // Arrange
            var invalidBytes = Array.Empty<byte>();

            // Act / Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new LightningDbCacheExpiry(invalidBytes));
        }
    }
}