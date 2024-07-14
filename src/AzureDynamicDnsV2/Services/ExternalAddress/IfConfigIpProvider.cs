namespace AzureDynamicDnsV2.Services.ExternalAddress;


public class IfConfigIpProvider : IExternalIpProvider
{
    private readonly HttpClient client;

    public IfConfigIpProvider(IHttpClientFactory httpClientFactory)
    {
        this.client = httpClientFactory.CreateClient(nameof(IExternalIpProvider));
    }

    public async Task<string> GetPublicIpAsync()
    {
        return await this.client.GetStringAsync("https://ifconfig.me");
    }
}