using System.Security.Cryptography;
using BenchmarkDotNet.Running;
using LightningDB;
using LightningDbCache;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

// LightningDB.LightningEnvironment environment = new LightningDB.LightningEnvironment(@"C:\users\sstrand\Desktop", new() { MapSize = 1014 * 1024 * 1024, MaxDatabases = 2, MaxReaders = 1024 });

// environment.Open();

// using(var tran = environment.BeginTransaction())
// {
//     var db = tran.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
//     var key = RandomNumberGenerator.GetBytes(24);
//     var bytes = RandomNumberGenerator.GetBytes(500);
//     tran.Put(db,key,bytes);
// }




// using LightningDbCache;
// using Microsoft.Extensions.Caching.Distributed;

// var options = new DistributedCacheEntryOptions()
// {
//     SlidingExpiration = TimeSpan.FromMinutes(10),
//     AbsoluteExpiration = DateTimeOffset.Now.AddDays(40),
//     AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(400)
// };

// var bytes = DistributedCacheEntryParser.ToBytes(options);

// Console.WriteLine(bytes.Length);