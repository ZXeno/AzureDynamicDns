namespace AzureDynamicDns.Services
{
    using System.Threading.Tasks;

    public interface IDnsService
    {
        /// <summary>
        /// Update method for the DNS service implementation.
        /// </summary>
        Task UpdateDns();

        /// <summary>
        /// Creates the default configuration file for the service implementation.
        /// </summary>
        Task CreateDefaultConfig();

        /// <summary>
        /// Loads the default configuration file if no config file path is provided..
        /// </summary>
        Task LoadServiceConfig(string providedConfig = null);

        /// <summary>
        /// Checks for a configuration file. This should check for the provided one first, then the default if none is provided.
        /// </summary>
        bool ConfigFileExists(string providedConfig = null);
    }
}