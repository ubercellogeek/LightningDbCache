using LightningDB;
using Microsoft.Extensions.Caching.Distributed;

namespace LightningDbCache.TestApp;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly PeriodicTimer _timer;
    private readonly IDistributedCache _cache;

    public Worker(ILogger<Worker> logger, IDistributedCache cache)
    {
        var config = new LightningDbCacheOptions();
        EnvironmentConfiguration env;
        env = config;


        

        _logger = logger;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await _cache.SetStringAsync("secretKeyl;alksdfjjjjjjjjjjjjjjjjjjjjjjjjjjjal;ksjdfkajsdkfj;aksdjf;kjasdkfj;alskdjf;kajsd;lfkja;sldkjf;laksjdf;lkjasd;lfkja;sldkjf;asjdf;ljasd;lfja;slkdjf;kajsd;fja;sldjf;lasjdf;ljasd;lfjla;skjdfl;kajsdflk;jsal;dfjkal;ksdjfl;ajsdfl;kjasdl;kfj;alskdfj;laksjdf;lkajsdflk;jasd;lkfj;asldkjf;laksdjf;lkajsdfl;kjas;dlfja;lsdkjf;laksjdf;lkajsd;fkljasd;lkfja;sdlkjf;laskdjfl;kajsdfl;kjasd;lfj;asldkjf;laksjdf;jas;dlfj;alskdjf;lasjdfl;jasd;lfjasl;dfj;lasjdfl;kjasd;lfjas;dlfj;asldkjf;lasdjf;lasjdf;lasjdf;ljasd;lfjasd;lfjlas;kdjf;lkasdjf;lkajsdf;lasd;fljasd;lfjasd;lfj;asdljf;lkasdjf;ljasd;lfkjasd;lfj;lasdkjf;laskjf;laskdjf;lasdjf;ljasd;fjas;dljfasdjf;lasjdf;lkjasdfjasd;lfjas;dlfja;sdlj", "secretValue", new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5) });
        while (!stoppingToken.IsCancellationRequested)
        {
            await _timer.WaitForNextTickAsync(stoppingToken);
            var value = await _cache.GetStringAsync("secretKey");
            _logger.LogInformation("Cached value: {value}", value);
        }
    }
}
