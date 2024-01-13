using LightningDbCache;
using LightningDbCache.TestApp;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.UseLightningDbCache();
        services.AddHostedService<Worker>();
    })
    .UseSerilog((context, _, config) =>
    {
        config.ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext();
    })
    .Build();

host.Run();
