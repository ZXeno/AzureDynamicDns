namespace AzureDynamicDns.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.Dns;
    using Microsoft.Azure.Management.Dns.Models;
    using Microsoft.Rest;
    using Microsoft.Rest.Azure.Authentication;

    public class AzureDnsService : IDnsService
    {
        private const string ConfigFileName = "azdns.json";
        private readonly IExternalIpProvider ipProvider;
        private readonly SimplisticLogger logger;
        private readonly string configFileDirectory;
        private readonly string rootDirectoryPath;
        private AzureConfig config;

        /// <summary>
        /// Instantiates a new instance of the class <see cref="AzureDnsService"/>
        /// </summary>
        /// <param name="logger">The <see cref="SimplisticLogger"/> dependency.</param>
        /// <param name="ipProvider">A provider for an external IP address.</param>
        public AzureDnsService(SimplisticLogger logger, IExternalIpProvider ipProvider)
        {
            this.logger = logger;
            this.ipProvider = ipProvider;
            this.rootDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                    
                this.configFileDirectory = ".config/azdns";
            }
            else
            {
                this.configFileDirectory = "AppData\\azdns\\";
            }
        }
        
        /// <summary>
        /// Performs the retrieval and update of the DNS record in the configuration.
        /// </summary>
        public async Task UpdateDns()
        {
            ServiceClientCredentials creds = await ApplicationTokenProvider.LoginSilentAsync(config.TenantId, config.ClientId, config.ClientSecret);
            var dnsClient = new DnsManagementClient(creds);
            dnsClient.SubscriptionId = config.SubscriptionId;

            // Get our external IP. If our external IP matches the last one, there's no need to update.
            var ip = await this.ipProvider.GetPublicIp();
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
                    { "updated", DateTimeOffset.UtcNow.ToString("yyyy-MM-dd hh:mm:ss zz", CultureInfo.InvariantCulture) }
                }
            };

            // Do the update
            try
            {
                var result = await dnsClient.RecordSets.CreateOrUpdateAsync(
                    this.config.ResourceGroup,
                    this.config.Zone,
                    this.config.Record,
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

        public bool ConfigFileExists(string providedConfig = null)
        {
            string configFilePath = string.Empty;
            
            if (providedConfig is null)
            {
                this.logger.LogInfoAsync("No config file path was provided. Using default config path.").ConfigureAwait(false);
                configFilePath = Path.Combine(this.rootDirectoryPath, this.configFileDirectory, ConfigFileName);
            }
            
            return File.Exists(configFilePath);
        }

        public async Task LoadServiceConfig(string providedConfig = null)
        {
            string configFilePath = string.Empty;
            
            if (providedConfig is null)
            {
                await logger.LogInfoAsync("No config file path was provided Using default.");
                configFilePath = Path.Combine(this.rootDirectoryPath, this.configFileDirectory, ConfigFileName);
            }

            string json = await File.ReadAllTextAsync(configFilePath);
            this.config = JsonSerializer.Deserialize<AzureConfig>(json) ?? throw new Exception("Deserializer returned a null result somehow.");
            this.config.ConfigFilePath = configFilePath;
        }

        public async Task CreateDefaultConfig()
        {
            try
            {
                string directoryPath = Path.Combine(this.rootDirectoryPath, this.configFileDirectory);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filepath = Path.Combine(directoryPath, ConfigFileName);
                await File.WriteAllTextAsync(filepath, AzureConfig.CreateJsonTemplate());
                await this.logger.LogInfoAsync($"A filled configuration file is required. A blank one has been created and placed at {filepath}");
            }
            catch (Exception e)
            {
                await this.logger.LogErrorAsync($"Unable to create configuration template!\n----------\n{e.Message}");
                await this.logger.LogInfoAsync("\nA configuration template should be in the working directory of this program by default. You may copy that and provide it vay the '-c' parameter like so: \"/path/to/executable -c /path/to/your/config_file.json\"", true);
            }
        }
    }
}