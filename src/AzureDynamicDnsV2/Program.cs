using AzureDynamicDnsV2.Services;
using AzureDynamicDnsV2.Services.Dns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddUserSecrets<Program>();
    })
    .ConfigureServices((context, services) =>
    {
        // TODO: OpenTelemetry support

        services.AddOptions<AzureOptions>().Configure<IConfiguration>((opts, config) =>
        {
            config.Bind(opts);
        });

        services.ConfigureServices(context.Configuration)
            .AddHostedService<DynDnsService>();

    })
    .Build();

await host.RunAsync();