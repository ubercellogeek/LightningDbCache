using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LightningDB;

namespace LightningDbCache
{
    public class LightningDbCache : IDistributedCache, IDisposable
    {
        private const long _minMapSize = 16384;
        internal readonly ILogger _logger;
        private readonly LightningDbCacheOptions _options;
        private readonly LightningEnvironment _environment;
        private LightningDatabase? _cacheDatabase;
        private LightningDatabase? _expiryDatabase;
        private DateTime _lastExpirationScan;
        private bool _disposed;
        private const string _cacheDatabaseName = "cache";
        private const string _expiryDatabaseName = "expiry";

        public LightningDbCache(IOptions<LightningDbCacheOptions> optionsAccessor, ILoggerFactory loggerFactory)
        {
            _options = optionsAccessor.Value;
            _logger = loggerFactory.CreateLogger<LightningDbCache>();

            if (_options.MaxSize < _minMapSize)
            {
                throw new ArgumentOutOfRangeException(nameof(optionsAccessor), $"MaxSize must be greater than {_minMapSize}. Supplied value was: {_options.MaxSize}.");
            }

            _options.DataPath ??= Directory.GetCurrentDirectory();
            _lastExpirationScan = DateTime.UtcNow;
            _environment = new LightningEnvironment(_options.DataPath, _options.EnvironmentConfiguration);
        }

        [ExcludeFromCodeCoverage]
        ~LightningDbCache() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public byte[]? Get(string key)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);

            CheckDisposed();
            EnsureOpened();

            var keyBytes = Encoding.UTF8.GetBytes(key);
            using (var tran = _environment.BeginTransaction())
            {
                if (tran.TryGet(_cacheDatabase, keyBytes, out var dataBytes) && tran.TryGet(_expiryDatabase, keyBytes, out var expiryBytes))
                {
                    var expiry = new LightningDbCacheExpiry(expiryBytes);

                    if (expiry.Expired)
                    {
                        StartScanForExpiredItemsIfNeeded(DateTime.UtcNow);
                        return null;
                    }

                    expiry.UpdateLastAccessed();
                    tran.Put(_expiryDatabase, keyBytes, expiry);
                    tran.Commit();

                    StartScanForExpiredItemsIfNeeded(DateTime.UtcNow);

                    return dataBytes;
                }

                StartScanForExpiredItemsIfNeeded(DateTime.UtcNow);

                return null;
            }
        }

        /// <inheritdoc />
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            return Task.FromResult(Get(key));
        }

        /// <inheritdoc />
        public void Refresh(string key)
        {
            _ = Get(key);
        }

        /// <inheritdoc />
        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            Refresh(key);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            ArgumentNullException.ThrowIfNull(key);

            CheckDisposed();
            EnsureOpened();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            using (var tran = _environment.BeginTransaction())
            {
                tran.Delete(_expiryDatabase, keyBytes);
                tran.Delete(_cacheDatabase, keyBytes);
                tran.Commit();
            }

            StartScanForExpiredItemsIfNeeded(DateTime.UtcNow);
        }

        /// <inheritdoc />
        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            ArgumentNullException.ThrowIfNull(key);

            CheckDisposed();
            EnsureOpened();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length > 511) throw new ArgumentOutOfRangeException("Keys are limited to maximum length of 511 characters.");

            var entry = new LightningDbCacheExpiry(options);

            using (var tran = _environment.BeginTransaction())
            {
                tran.Put(_expiryDatabase, keyBytes, entry);
                tran.Put(_cacheDatabase, keyBytes, value);
                tran.Commit();
            }

            StartScanForExpiredItemsIfNeeded(DateTime.UtcNow);
        }

        /// <inheritdoc />
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispose the cache.
        /// </summary>
        /// <param name="disposing">Dispose the object resources if true; otherwise, take no action.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _environment.Dispose();
                    GC.SuppressFinalize(this);
                }

                _disposed = true;
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) Throw();

            static void Throw() => throw new ObjectDisposedException(typeof(LightningDbCache).FullName);
        }

        private void StartScanForExpiredItemsIfNeeded(DateTime utcNow)
        {
            if (_options.ExpirationScanFrequency < utcNow - _lastExpirationScan)
            {
                ScheduleTask(utcNow);
            }

            void ScheduleTask(DateTime utcNow)
            {
                _lastExpirationScan = utcNow;
                Task.Factory.StartNew(task => CleanupExpiredEntries(), this,
                    CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }
        }

        private void CleanupExpiredEntries()
        {
            Debug("Starting cleanup of expired entries.");
            using (var tran = _environment.BeginTransaction())
            using (var cursor = tran.CreateCursor(_cacheDatabase))
            {
                foreach (var entry in cursor.AsEnumerable())
                {
                    var key = entry.Item1.AsSpan();
                    var value = entry.Item2.AsSpan();

                    if (tran.TryGet(_expiryDatabase, key, out var expiryBytes))
                    {
                        var expiry = (LightningDbCacheExpiry)expiryBytes;

                        if (expiry.Expired)
                        {
                            var keyString = Encoding.UTF8.GetString(key);
                            cursor.Delete();
                            tran.Delete(_expiryDatabase, key);
                        }
                    }
                }
                tran.Commit();
            }
            Debug("Finished cleanup of expired entries.");
        }

        private void EnsureOpened()
        {
            if (!_environment.IsOpened)
            {
                _environment.Open();

                using var tran = _environment.BeginTransaction();
                {
                    _cacheDatabase = tran.OpenDatabase(_cacheDatabaseName, new DatabaseConfiguration() { Flags = DatabaseOpenFlags.Create });
                    _expiryDatabase = tran.OpenDatabase(_expiryDatabaseName, new DatabaseConfiguration() { Flags = DatabaseOpenFlags.Create });

                    tran.Commit();
                }
            }
        }

        [ExcludeFromCodeCoverage]
        private void Debug(string? message, params object?[] args)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(message, args);
            }
        }
    }
}
