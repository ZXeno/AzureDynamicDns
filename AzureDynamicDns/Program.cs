namespace AzureDynamicDns
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;

    public class Program
    {
        private const string ConfigFileName = "azdns.json";
        private static SimplisticLogger logger = null;

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

            // Check for config file parameter. If the path provided by command line is null,
            // then check for one by OS location.
            if (string.IsNullOrEmpty(options.ConfigFile))
            {
                await logger.LogInfoAsync("No config file path was provided Using default.");
                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    options.ConfigFile = Path.Combine(Environment.SpecialFolder.UserProfile.ToString(), "/.config/azdns/", ConfigFileName);
                }
                else
                {
                    options.ConfigFile = Path.Combine(Environment.SpecialFolder.UserProfile.ToString(), "\\AppData\\azdns\\", ConfigFileName);
                }
            }

            // Ensure that config exists. 
            if (!File.Exists(options.ConfigFile))
            {
                await logger.LogErrorAsync($"Could not find a config file at {options.ConfigFile}");
                
                await logger.LogInfoAsync("Configuration file not found!");
                try
                {
                    await File.WriteAllTextAsync(options.ConfigFile, AzureConfig.CreateJsonTemplate());
                }
                catch (Exception e)
                {
                    await logger.LogErrorAsync($"Unable to create configuration template:\n{e.Message}");
                    await logger.LogInfoAsync("\nA configuration template should be in the working directory of this program by default. You may copy that and provide it vay the '-c' parameter like so: \"/path/to/executable -c /path/to/your/config_file.json\"", true);
                }

                await logger.LogInfoAsync($"A configuration file is required. A blank one has been created and placed at {options.ConfigFile}");
                return;
            }

            // Read and parse config
            AzureConfig config = null;
            try
            {
                string json = await File.ReadAllTextAsync(options.ConfigFile);
                config = JsonSerializer.Deserialize<AzureConfig>(json) ?? throw new Exception("Deserializer returned a null result somehow.");
                config.ConfigFilePath = options.ConfigFile;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to parse configuration file!\n{e.Message}");
            }

            DnsService dns = new DnsService(config, logger);
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
