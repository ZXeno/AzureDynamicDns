namespace AzureDynamicDns
{
    using System.Text.Json.Serialization;

    public class AzureConfig
    {
        [JsonPropertyName("resourceGroup")]
        public string ResourceGroup { get; set; }
        
        [JsonPropertyName("zoneName")]
        public string Zone { get; set; }
        
        [JsonPropertyName("recordName")]
        public string Record { get; set; }
        
        [JsonPropertyName("subscriptionId")]
        public string SubscriptionId { get; set; }
        
        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; }
        
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }

        [JsonPropertyName("clientSecret")]
        public string ClientSecret { get; set; }
    }
}
