using System.Text;
using LightningDB;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LightningDbCache
{
    public class LightningDbCache : IDistributedCache
    {
        internal readonly ILogger _logger;
        private readonly LightningDbCacheOptions _options;
        private readonly LightningEnvironment _environment;
        private readonly LightningDatabase _database;

        public LightningDbCache(IOptions<LightningDbCacheOptions> optionsAccessor) : this(optionsAccessor, NullLoggerFactory.Instance)
        {

        }

        public LightningDbCache(IOptions<LightningDbCacheOptions> optionsAccessor, ILoggerFactory loggerFactory)
        {
            var config = new DatabaseConfiguration();

            _options = optionsAccessor.Value;
            _logger = loggerFactory.CreateLogger<LightningDbCache>();

            var environmentConfiguration = new EnvironmentConfiguration()
            {
                MaxDatabases = 2,
                MaxReaders = 1024,
                MapSize = _options.MapSize = _options.MapSize
            };

            _environment = new LightningEnvironment(_options.BasePath, environmentConfiguration);

            using var tran = _environment.BeginTransaction();
            {
                _database = tran.OpenDatabase("cache", new DatabaseConfiguration() { Flags = DatabaseOpenFlags.Create });
                tran.Commit();
            }
        }

        public byte[]? Get(string key)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);

            var keyByte = System.Text.Encoding.UTF8.GetBytes(key);
            using(var tran = _environment.BeginTransaction())
            {
                
                tran.TryGet(_database, keyByte, out var value);
                tran.Commit();
                return value;
            }
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            return Task.FromResult(Get(key));
        }

        public void Refresh(string key)
        {
            _ = Get(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            _ = Get(key);
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            var keyByte = Encoding.UTF8.GetBytes(key);
            using(var tran = _environment.BeginTransaction())
            {
                tran.Delete(_database, keyByte);
                tran.Commit();
            }
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var keyByte = System.Text.Encoding.UTF8.GetBytes(key);
            using(var tran = _environment.BeginTransaction())
            {
                tran.Put(_database, keyByte, value);
                tran.Commit();
            }
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }
    }
}