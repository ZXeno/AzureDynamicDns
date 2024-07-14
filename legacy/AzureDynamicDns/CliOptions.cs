namespace AzureDynamicDns
{
    using CommandLine;
    
    public class CliOptions
    {
        [Option('c', "configfile",
            Required = false,
            HelpText = "Set the config file to use. Otherwise, the default location is used. On Windows, this is <%userprofile%\\AppData\\Roaming\\azdns\\azdns.json>. In Linux/Osx, it is <~/.config/azdns/azdns.json>")]
        public string ConfigFile { get; set; }

        [Option('r', "rate",
            Required = false,
            HelpText = "The rate in seconds at which to update. Default is 600 seconds (10 minutes).")]
        public int Rate { get; set; } = 600;
        
        [Option('l', "logfile",
        Required = false,
        HelpText = "This parameter provides two functions in one: Indicates that a logfile should be written, and where to write said logfile. If the value 'default' is provided, then the working directory is used. Otherwise, specify a log file location and sure that the application has write access to that location.")]
        public string LogFile { get; set; }
    }
}