namespace AzureDynamicDns
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CommandLine;
    using Microsoft.Azure.Management.Dns;
    using Microsoft.Azure.Management.Dns.Models;
    using Microsoft.Rest.Azure.Authentication;

    public class Program
    {
        private const string JsonFile = "azdns.json";

        public static async Task Main(string[] args)
        {
            CliOptions options = null;

            Parser.Default.ParseArguments<CliOptions>(args).WithParsed((o) => { options = o; });

            string filePath = string.IsNullOrEmpty(options.ConfigFile) ?
                Path.Combine(Environment.CurrentDirectory, JsonFile) : options.ConfigFile;

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found!");
                return;
            }

            string json = await File.ReadAllTextAsync(filePath);
            AzureConfig config = JsonSerializer.Deserialize<AzureConfig>(json);

            await UpdateDns(config);
        }

        private static async Task UpdateDns(AzureConfig options)
        {
            var creds = await ApplicationTokenProvider.LoginSilentAsync(options.TenantId, options.ClientId, options.ClientSecret);
            var dnsClient = new DnsManagementClient(creds);
            dnsClient.SubscriptionId = options.SubscriptionId;

            var ip = await GetPublicIp();
            var recordSet = new RecordSet
            {
                TTL = 3600,
                ARecords = new List<ARecord>() { new ARecord(ip) },
                Metadata = new Dictionary<string, string>()
                {
                    { "createdBy", "AzureDynamicDns" },
                    { "updated", DateTime.Now.ToString(CultureInfo.InvariantCulture) }
                }
            };

            var result = await dnsClient.RecordSets.CreateOrUpdateAsync(options.ResourceGroup, options.Zone, options.Record, RecordType.A, recordSet);

            Console.WriteLine(JsonSerializer.Serialize(result, options: new JsonSerializerOptions() { WriteIndented = true }));
        }

        private static async Task<string> GetPublicIp()
        {
            var client = new HttpClient();
            return await client.GetStringAsync("https://ifconfig.me");
        }
    }
}
