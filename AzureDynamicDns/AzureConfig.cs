namespace AzureDynamicDns
{
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public class AzureConfig
    {
        [JsonIgnore]
        const string DefaultValue = "<insert_value_here>";
        
        [JsonIgnore]
        public string ConfigFilePath { get; set; }
        
        [JsonPropertyName("resourceGroup")]
        public string ResourceGroup { get; set; } = DefaultValue;
        
        [JsonPropertyName("zoneName")]
        public string Zone { get; set; } = DefaultValue;
        
        [JsonPropertyName("recordName")]
        public string Record { get; set; } = DefaultValue;
        
        [JsonPropertyName("subscriptionId")]
        public string SubscriptionId { get; set; } = DefaultValue;
        
        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = DefaultValue;
        
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = DefaultValue;

        [JsonPropertyName("clientSecret")]
        public string ClientSecret { get; set; } = DefaultValue;
        
        /// <summary>
        /// This is used to cache the last recorded IP.
        /// </summary>
        [JsonPropertyName("lastRecordedIp")]
        public string LastRecordedIp { get; set; } = null;

        public static string CreateJsonTemplate()
        {
            return JsonSerializer.Serialize(
                new AzureConfig(),
                options: new JsonSerializerOptions() { WriteIndented = true });
        }

        public async Task WriteToFileAsync(string filePath)
        {
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(this, options: new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}
