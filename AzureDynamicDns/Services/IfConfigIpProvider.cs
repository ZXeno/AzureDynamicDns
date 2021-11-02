namespace AzureDynamicDns.Services
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public class IfConfigIpProvider : IExternalIpProvider
    {
        public async Task<string> GetPublicIpAsync()
        {
            var client = new HttpClient();
            return await client.GetStringAsync("https://ifconfig.me");
        }
    }
}