namespace AzureDynamicDns.Services
{
    using System.Threading.Tasks;

    public interface IDnsService
    {
        Task UpdateDns();

        Task CreateDefaultConfig();

        Task LoadServiceConfig(string providedConfig = null);

        bool ConfigFileExists(string providedConfig = null);
    }
}