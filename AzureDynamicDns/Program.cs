namespace AzureDynamicDns
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;
    using Services;

    public class Program
    {
        private static SimplisticLogger logger;

        public static async Task Main(string[] args)
        {
            // Parse our options first. This library allows us to pass parsed arguments directly to
            // a function, but I want to use those manually before initiating the primary program
            // functionality.
            CliOptions options = null;
            Parser.Default.ParseArguments<CliOptions>(args).WithParsed((o) => { options = o; });
            
            // Activate the basic logger
            logger = new SimplisticLogger(options.LogFile);
            logger.Init();

            IExternalIpProvider ipProvider = new IfConfigIpProvider();
            IDnsService dns = new AzureDnsService(logger, ipProvider);

            // Ensure that config exists. 
            if (!dns.ConfigFileExists(options.ConfigFile))
            {
                // If the config doesn't exist, create a blank config and exit.
                await logger.LogInfoAsync("Configuration file not found!\nA template file will be created.");
                await dns.CreateDefaultConfig();
                return;
            }

            // Load the provided DNS service config.
            await dns.LoadServiceConfig(options.ConfigFile);
            
            while (true)
            {
                try
                {
                    await dns.UpdateDns();
                    Thread.Sleep(options.Rate * 1000);
                }
                catch (Exception e)
                {
                    await logger.LogErrorAsync($"Exiting due to error:\n{e.Message}");
                    break;
                }
            }
        }
    }
}
