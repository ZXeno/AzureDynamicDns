namespace AzureDynamicDnsV2.Services.Dns;

public interface IAzureDynDnsService
{
    Task UpdateDns(CancellationToken cancellationToken = default);
}