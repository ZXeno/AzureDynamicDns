using AzureDynamicDnsV2.Options;
using AzureDynamicDnsV2.Services.Dns;
using AzureDynamicDnsV2.Services.ExternalAddress;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDynamicDnsV2.Services;

public static class ServiceBootstrapper
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IfConfigOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.Bind(settings);
            });

        services.AddScoped<IExternalIpProvider, IfConfigIpProvider>();
        services.AddScoped<IAzureDynDnsService, AzureDynDnsService>();

        // Register named HttpClient to benefit from IHttpClientFactory
        services.AddHttpClient(nameof(IExternalIpProvider));

        return services;
    }
}
