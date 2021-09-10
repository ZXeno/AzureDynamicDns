namespace AzureDynamicDns
{
    using CommandLine;
    
    public class CliOptions
    {
        [Option('c', "configfile", Required = false, HelpText = "Set the config file to use. Otherwise, the default location is used.")]
        public string ConfigFile { get; set; }

        [Option('r', "rate", Required = false, HelpText = "The rate in seconds at which to update. Default is 600 seconds (10 minutes).")]
        public int Rate { get; set; } = 600;
        
        
    }
}