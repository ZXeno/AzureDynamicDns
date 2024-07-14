#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.

namespace AzureDynamicDnsV2.Services.Dns;

public sealed class AzureOptions
{
    public string CreatedBy { get; set; } = "AzureDynamicDnsV2";

    public string RecordNames { get; set; }

    public string ResourceGroup { get; set; }

    public string SubscriptionId { get; set; }

    public int TimeToLive { get; set; }

    public string ZoneName { get; set; }

    public string AzureRegion { get; set; }

    public int UpdateInterval { get; set; }
}