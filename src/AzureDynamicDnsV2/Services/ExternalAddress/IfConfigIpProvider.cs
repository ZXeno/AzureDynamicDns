using AzureDynamicDnsV2.Options;
using Microsoft.Extensions.Options;

namespace AzureDynamicDnsV2.Services.ExternalAddress;


public sealed class IfConfigIpProvider : IExternalIpProvider
{
    private readonly HttpClient client;
    private readonly IfConfigOptions ifConfig;

    public IfConfigIpProvider(IHttpClientFactory httpClientFactory, IOptions<IfConfigOptions> opts)
    {
        this.client = httpClientFactory.CreateClient(nameof(IExternalIpProvider));
        this.ifConfig = opts.Value;
        ArgumentNullException.ThrowIfNullOrWhiteSpace(this.ifConfig.IfConfigUrl);
    }

    public async Task<string> GetPublicIpAsync()
    {
        return await this.client.GetStringAsync(this.ifConfig.IfConfigUrl);
    }
}