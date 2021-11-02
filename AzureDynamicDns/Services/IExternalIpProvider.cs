namespace AzureDynamicDns.Services
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface exposing a function for acquiring external IP addresses from an implementation.
    /// </summary>
    public interface IExternalIpProvider
    {
        /// <summary>
        /// Asynchronously retrieves an IP address from an external provider.
        /// </summary>
        Task<string> GetPublicIpAsync();
    }
}