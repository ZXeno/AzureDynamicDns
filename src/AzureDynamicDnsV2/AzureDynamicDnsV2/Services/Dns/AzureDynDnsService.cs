using System.Net;
using Azure;
using Azure.Core;
using AzureDynamicDnsV2.Services.ExternalAddress;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.ResourceManager.Dns;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;


namespace AzureDynamicDnsV2.Services.Dns;

public sealed class AzureDynDnsService : IAzureDynDnsService
{
    private readonly ILogger<AzureDynDnsService> logger;
    private readonly IExternalIpProvider ipProvider;
    private readonly AzureOptions config;
    private string lastSeenIp;
    private string[] recordNames;

    public AzureDynDnsService(ILogger<AzureDynDnsService> logger, IExternalIpProvider ipProvider, IOptions<AzureOptions> options)
    {
        this.logger = logger;
        this.ipProvider = ipProvider;
        this.config = options.Value;
        this.lastSeenIp = string.Empty;
        if (this.config.RecordNames.IndexOf(',') <= 0)
        {
            this.recordNames = new[] { this.config.RecordNames
                .Replace(" ", string.Empty)
                .Replace("", string.Empty) };
            return;
        }

        this.recordNames = this.config.RecordNames.Replace(" ", string.Empty).Split(',');
    }

    public async Task UpdateDns(CancellationToken cancellationToken = default)
    {
        string retrievedIp = await this.ipProvider.GetPublicIpAsync();
        if (this.lastSeenIp == retrievedIp)
        {
            // nothing has changed.
            return;
        }

        this.lastSeenIp = retrievedIp;

        // get azure clients
        var creds = new EnvironmentCredential();
        ArmClient client = new ArmClient(creds);

        ResourceIdentifier? rid = ResourceGroupResource.CreateResourceIdentifier(
            this.config.SubscriptionId,
            this.config.ResourceGroup);

        ResourceGroupResource resourceGroup = client.GetResourceGroupResource(rid);

        // Check for our DNS zone
        if (!await resourceGroup.GetDnsZones().ExistsAsync(this.config.ZoneName, cancellationToken))
        {
            this.logger.LogError("Zone {ZoneName} does not exist in Resource Group {ResourceGroup}.",
                this.config.ZoneName,
                this.config.ResourceGroup);

            return;
        }


        // Get our zone data/zone client
        Response<DnsZoneResource>? response = await resourceGroup.GetDnsZoneAsync(this.config.ZoneName, cancellationToken);
        if (response is null || !response.Value.HasData)
        {
            this.logger.LogError("Unable to pull data for zone {ZoneName} in Resource Group {ResourceGroup}",
                this.config.ZoneName,
                this.config.ResourceGroup);

            return;
        }

        DnsZoneResource zone = response.Value;

        if (this.recordNames.Length <= 1)
        {
            await this.UpdateRecord(this.recordNames[0], retrievedIp, zone, cancellationToken);
            return;
        }

        // if we have more than one, just run them all in parallel.
        Task[] updateTasks = new Task[this.recordNames.Length];
        for (int x = 0; x < this.recordNames.Length; x++)
        {
            Task updateTask = this.UpdateRecord(this.recordNames[x], retrievedIp, zone, cancellationToken);
            updateTasks[x] = updateTask;
        }
        Task.WaitAll(updateTasks, cancellationToken);
    }

    private async Task UpdateRecord(string recordName, string ipAddress, DnsZoneResource zone, CancellationToken cancellationToken = default)
    {
        DnsARecordData record = new()
        {
            TtlInSeconds = this.config.TimeToLive,
            DnsARecords =
            {
                new()
                {
                    IPv4Address = IPAddress.Parse(ipAddress),
                },
            },
            Metadata =
            {
                { "createdBy", this.config.CreatedBy },
                { "updated", TimeProvider.System.GetUtcNow().ToString("u") },
            },
        };

        // Set our record
        ArmOperation<DnsARecordResource>? recordResponse = await zone.GetDnsARecords().CreateOrUpdateAsync(WaitUntil.Completed, recordName, record, cancellationToken: cancellationToken);
        if (recordResponse.HasCompleted && !recordResponse.HasValue)
        {
            this.logger.LogError("The update process completed, but failed to return a response value. ARM Operation Id: {OperationId}", recordResponse.Id);
        }

        DnsARecordResource aRecord = recordResponse.Value;
        this.logger.LogInformation("The record was updated successfully. New IP: {IpAddress} Response: {RecordName}", ipAddress, aRecord.Data.Name);
    }
}