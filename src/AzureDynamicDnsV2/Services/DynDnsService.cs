using AzureDynamicDnsV2.Services.Dns;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureDynamicDnsV2.Services;

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
        if (!this.ValidateConfig())
        {
            return;
        }

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

    private bool ValidateConfig()
    {
        if (string.IsNullOrWhiteSpace(this.config.ResourceGroup))
        {
            this.logger.LogError("Invalid {ParameterName} provided! Exiting", nameof(this.config.ResourceGroup));
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.config.SubscriptionId) && !Guid.TryParse(this.config.SubscriptionId, out _))
        {
            this.logger.LogError("Invalid {ParameterName} provided! Exiting", nameof(this.config.SubscriptionId));
            return false;
        }

        string tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID")!;
        if (string.IsNullOrWhiteSpace(tenantId) && !Guid.TryParse(tenantId, out _))
        {
            this.logger.LogError("Invalid {ParameterName} provided! Exiting", nameof(this.config.SubscriptionId));
            return false;
        }

        string clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")!;
        if (string.IsNullOrWhiteSpace(clientId) && !Guid.TryParse(clientId, out _))
        {
            this.logger.LogError("Invalid {ParameterName} provided! Exiting", nameof(this.config.SubscriptionId));
            return false;
        }

        string clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")!;
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            this.logger.LogError("Invalid {ParameterName} provided! Exiting", nameof(this.config.SubscriptionId));
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.config.SubscriptionId) && !Guid.TryParse(this.config.SubscriptionId, out _))
        {
            this.logger.LogError("Invalid {ParameterName} provided! Exiting", nameof(this.config.SubscriptionId));
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.config.SubscriptionId))
        {
            this.logger.LogError("Invalid {ParameterName} provided! Exiting", nameof(this.config.ZoneName));
            return false;
        }

        if (this.config.UpdateInterval <= 0 && this.config.UpdateInterval >= int.MaxValue)
        {
            this.logger.LogError("{ParameterName} must be a valid Int32 value greater than {MinValue} and less than {MaxValue}. Exiting",
                nameof(this.config.UpdateInterval),
                0,
                int.MaxValue);

            return false;
        }

        if (this.config.TimeToLive <= 0 && this.config.TimeToLive >= int.MaxValue)
        {
            this.logger.LogError("{ParameterName} must be a valid Int32 value greater than {MinValue} and less than {MaxValue}. Exiting",
                nameof(this.config.TimeToLive),
                0,
                int.MaxValue);

            return false;
        }

        return true;
    }
}