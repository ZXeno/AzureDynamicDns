namespace AzureDynamicDns.Services
{
    using System.Threading.Tasks;

    public interface IExternalIpProvider
    {
        Task<string> GetPublicIp();
    }
}