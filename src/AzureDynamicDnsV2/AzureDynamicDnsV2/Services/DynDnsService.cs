using AzureDynamicDnsV2.Services.Dns;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace AzureDynamicDnsV2.Services;

using Microsoft.Extensions.Hosting;

public sealed class DynDnsService : BackgroundService
{
    private readonly ILogger<DynDnsService> logger;
    private readonly IAzureDynDnsService azDynDnsService;
    private readonly AzureOptions config;

    public DynDnsService(ILogger<DynDnsService> logger,
        IAzureDynDnsService azDynDnsService,
        IOptions<AzureOptions> options)
    {
        this.logger = logger;
        this.azDynDnsService = azDynDnsService;
        this.config = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                await this.azDynDnsService.UpdateDns(cancellationToken).ConfigureAwait(false);
                Thread.Sleep(this.config.UpdateInterval * 1000);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Exiting due to error:\n{ErrorMessage}", ex.Message);
                break;
            }
        }
    }
}