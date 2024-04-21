# LightningDbCache
[![build](https://github.com/ubercellogeek/LightningDbCache/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/ubercellogeek/LightningDbCache/actions/workflows/ci.yml) [![Coverage Status](https://coveralls.io/repos/github/ubercellogeek/LightningDbCache/badge.svg?branch=main)](https://coveralls.io/github/ubercellogeek/LightningDbCache?branch=main)

LightningDbCache is an implementation of [IDistributedCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache) that uses LightningDb as a backing store. LightningDb is a fast, transactional, memory-mapped, key-value store with a very small footprint. LightningDbCache is made possible by leveraging the [Lightning.NET](https://github.com/CoreyKaylor/Lightning.NET) library to facilitate the interplay with LightningDb itself.

## Getting Started

### Installation

Add a reference to the `LightningDbCache` package to your project

```bash
dotnet add package LightningDbCache
```
### Basic Usage
```csharp
// Register the cache in the DI container
services.AddLightningDbCache();
```

> **NOTE:** This registers a *singleton* instance implementation of `IDistributedCache`.

```csharp  
// Inject the cache into your class
public class MyService
{
    private readonly IDistributedCache _cache;

    public MyService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task DoSomethingAsync()
    {
        // Store a value in the cache
        await _cache.SetStringAsync("myKey", "myValue");

        // Retrieve a value from the cache
        var value = await _cache.GetStringAsync("myKey");
    }
}
```

## Configuration

The following configuration options are supported: 

| Option | Datatype |Description | Default Value |
| --- | --- |--- | --- |
| `DataPath` | `string` | The directory where the cache database files will be stored. | `Directory.GetCurrentDirectory()` |
| `MaxSize` | `long` | The maximum size of the cache database in bytes. | `209715200` (200MB) |
| `ExpirationScanFrequency` | `Timespan` | The frequency at which the cache will be scanned for expired items. | `TimeSpan.FromMinutes(1)` |

### Examples

#### Configure the cache from `appsettings.json`

```json
{
  "LightningDbCache": {
    "DataPath": "C:\\data\\mycache",
    "MaxSize": 100000000,
    "ExpirationScanFrequency": "00:00:30"
  }
}
```

```csharp
services.AddLightningDbCache();
```

#### Configure the cache from `appsettings.json` with custom section name

```json
{
  "MyCache": {
    "DataPath": "/data/mycache",
    "MaxSize": 100000000,
    "ExpirationScanFrequency": "00:00:30"
  }
}
```

```csharp
services.AddLightningDbCache("MyCache");
// or
services.AddLightningDbCache(configuration.GetSection("MyCache"));
```

#### Configure the cache from code

```csharp
services.AddLightningDbCache(options =>
{
    options.DataPath = "C:\\data\\mycache";
    options.MaxSize = 100000000;
    options.ExpirationScanFrequency = TimeSpan.FromSeconds(30);
});
```

## Notes

### Expiration

This library fully implements `IDistributedCache` expiration options. While expired items will never be returned from the cache, they will not be removed from the backing store until the next time the cache is scanned for expired items. This is done to avoid the overhead of removing items from the backing store on every cache access.

### Database Size

LightningDb is a [memory-mapped](https://en.wikipedia.org/wiki/Lightning_Memory-Mapped_Database) database. This means that the consumed database size in memory will also be reflected in the size of files on disk and vice versa. If you are using this library in a memory-constrained and/or disk space constrained environment, you should set the `MaxSize` option to a value that is appropriate for your application.

> **IMPORTANT:** The database file size on disk (and in memory) will grow up-to the `MaxSize` value. If the database size reaches the `MaxSize` value, new items will not be added to the cache until enough items are removed to bring the database size below the `MaxSize` value.

### Multithreading and Multiple Processes

Public members of `LightningDbCache` are thread-safe. 

While it is possible, care should be taken when sharing a cache database between multiple processes. Some filesystems may not support memory-mapped files in a way that is compatible with LightningDb. For more in-depth reading on LightningDb's threading and multiple process support, see the [LightningDb Docs](http://www.lmdb.tech/doc/)