namespace AzureDynamicDns
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Dns;
    using Microsoft.Azure.Management.Dns.Models;
    using Microsoft.Rest.Azure.Authentication;

    class Program
    {
        private const string JsonFile = "azdns.json";

        public static async Task Main(string[] args)
        {
            string expectedFile = Path.Combine(Environment.CurrentDirectory, JsonFile);

            if (!File.Exists(expectedFile))
            {
                Console.WriteLine("File not found!");
                return;
            }

            string json = File.ReadAllText(expectedFile);

            AzureConfig config = JsonSerializer.Deserialize<AzureConfig>(json);

            await UpdateDNS(config);
        }

        public static async Task UpdateDNS(AzureConfig options)
        {
            var creds = await ApplicationTokenProvider.LoginSilentAsync(options.TenantId, options.ClientId, options.ClientSecret);
            var dnsClient = new DnsManagementClient(creds);
            dnsClient.SubscriptionId = options.SubscriptionId;

            var ip = await GetPublicIP();
            var recordSet = new RecordSet
            {
                TTL = 3600,
                ARecords = new List<ARecord>() { new ARecord(ip) },
                Metadata = new Dictionary<string, string>()
                {
                    { "createdBy", "AzureDynamicDns" },
                    { "updated", DateTime.Now.ToString() }
                }
            };

            var result = await dnsClient.RecordSets.CreateOrUpdateAsync(options.ResourceGroup, options.Zone, options.Record, RecordType.A, recordSet);

            Console.WriteLine(JsonSerializer.Serialize(result, options: new JsonSerializerOptions() { WriteIndented = true }));
        }

        public static async Task<string> GetPublicIP()
        {
            var client = new HttpClient();
            return await client.GetStringAsync("https://ifconfig.me");
        }
    }
}
