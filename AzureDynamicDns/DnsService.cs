namespace AzureDynamicDns
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Dns;
    using Microsoft.Azure.Management.Dns.Models;
    using Microsoft.Rest.Azure.Authentication;

    public class DnsService
    {
        private readonly SimplisticLogger logger;
        private AzureConfig config;

        /// <summary>
        /// Instantiates a new instance of the class <see cref="DnsService"/>
        /// </summary>
        /// <param name="configuration">The <see cref="AzureConfig"/> representing connection options for the AzureDNS service.</param>
        /// <param name="logger">The <see cref="SimplisticLogger"/> dependency.</param>
        public DnsService(AzureConfig configuration, SimplisticLogger logger)
        {
            this.logger = logger;
            this.config = configuration;
        }
        
        /// <summary>
        /// Performs the retrieval and update of the DNS record in the configuration.
        /// </summary>
        public async Task UpdateDns()
        {
            var creds = await ApplicationTokenProvider.LoginSilentAsync(config.TenantId, config.ClientId, config.ClientSecret);
            var dnsClient = new DnsManagementClient(creds);
            dnsClient.SubscriptionId = config.SubscriptionId;

            // Get our external IP. If our external IP matches the last one, there's no need to update.
            var ip = await GetPublicIp();
            if (ip == this.config.LastRecordedIp)
            {
                return;
            }
            
            // Prepare the DNS record to write.
            var recordSet = new RecordSet
            {
                TTL = 3600,
                ARecords = new List<ARecord>() { new(ip) },
                Metadata = new Dictionary<string, string>()
                {
                    { "createdBy", "AzureDynamicDns" },
                    { "updated", DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture) }
                }
            };

            // Do the update
            try
            {
                var result = await dnsClient.RecordSets.CreateOrUpdateAsync(
                    config.ResourceGroup,
                    config.Zone,
                    config.Record,
                    RecordType.A,
                    recordSet);
                
                await logger.LogInfoAsync(
                    JsonSerializer.Serialize(
                        result,
                        options: new JsonSerializerOptions() {WriteIndented = true}));
            }
            catch (Exception e)
            {
                await logger.LogErrorAsync($"Unable to update record: {e.Message}");
            }

            // Log the result and save our last updated IP to the config.
            try
            {
                await this.logger.LogSuccess($"Updated DNS record with new IP: {ip}");
                this.config.LastRecordedIp = ip;
                await this.config.WriteToFileAsync(this.config.ConfigFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Gets the public IP from `ifconfig.me`.
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetPublicIp()
        {
            var client = new HttpClient();
            return await client.GetStringAsync("https://ifconfig.me");
        }
    }
}