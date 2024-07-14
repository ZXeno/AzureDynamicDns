namespace AzureDynamicDns
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// An extremely simplistic logger that formats and outputs info in a style I prefer to read.
    /// The weight of a proper logging framework is overtly excessive for this application and is intentionally not being used.
    /// If you disagree with this assessment, by all means, please implement one :)
    /// </summary>
    public class SimplisticLogger
    {
        private readonly string logFilePath;
        private readonly bool writeLogfile;
        private readonly string dateTimeFormat;

        public ConsoleColor InformationColor = ConsoleColor.DarkCyan;
        public ConsoleColor ErrorColor = ConsoleColor.DarkRed;
        public ConsoleColor SuccessColor = ConsoleColor.Green;

        /// <summary>
        /// Creates a new isntance of the <see cref="SimplisticLogger"/> class.
        /// </summary>
        public SimplisticLogger(string logFilePath, string dateTimeFormat = "yyyy-MM-dd hh:mm:ss zz")
        {
            this.logFilePath = logFilePath;
            this.writeLogfile = !string.IsNullOrEmpty(this.logFilePath);
            this.dateTimeFormat = dateTimeFormat;
        }

        /// <summary>
        /// Initializes the <see cref="SimplisticLogger"/>.
        /// Checks for the log file and creates it if not.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Throws if the logfile path is inaccessible to the application.</exception>
        public void Init()
        {
            if (!this.writeLogfile || File.Exists(this.logFilePath))
            {
                return;
            }

            try
            {
                File.CreateText(this.logFilePath);
            }
            catch (UnauthorizedAccessException e)
            {
                this.LogErrorAsync(e.Message, true).Wait();
                throw;
            }
            catch (Exception e)
            {
                this.LogErrorAsync(e.Message, true).Wait();
                throw;
            }
        }

        /// <summary>
        /// Performs Information level logging.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="skipLogOutput"></param>
        public async Task LogInfoAsync(string msg, bool skipLogOutput = false)
        {
            Console.ForegroundColor = this.InformationColor;
            string messageText = $"[{DateTimeOffset.UtcNow.ToString(dateTimeFormat, CultureInfo.InvariantCulture)}][INFO] {msg}";
            Console.WriteLine(messageText);
            Console.ResetColor();

            if (this.writeLogfile && !skipLogOutput)
            {
                try
                {
                    await File.AppendAllTextAsync(this.logFilePath, messageText);
                }
                catch
                {
                    await LogErrorAsync($"Unable to write to log file!", true);
                }

            }
        }

        /// <summary>
        /// Performs Error level logging.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="skipLogOutput"></param>
        public async Task LogErrorAsync(string msg, bool skipLogOutput = false)
        {
            Console.ForegroundColor = this.ErrorColor;
            string messageText = $"[{DateTimeOffset.UtcNow.ToString(dateTimeFormat, CultureInfo.InvariantCulture)}][ERROR] {msg}";
            Console.WriteLine(messageText);
            Console.ResetColor();

            if (this.writeLogfile && !skipLogOutput)
            {
                try
                {
                    await File.AppendAllTextAsync(this.logFilePath, messageText);
                }
                catch
                {
                    await LogErrorAsync($"Unable to write to log file!", true);
                }

            }
        }

        /// <summary>
        /// Performs Success level logging.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="skipLogOutput"></param>
        public async Task LogSuccess(string msg, bool skipLogOutput = false)
        {
            Console.ForegroundColor = this.SuccessColor;
            string messageText = $"[{DateTimeOffset.UtcNow.ToString(dateTimeFormat, CultureInfo.InvariantCulture)}][SUCCESS] {msg}";
            Console.WriteLine(messageText);
            Console.ResetColor();

            if (this.writeLogfile && !skipLogOutput)
            {
                try
                {
                    await File.AppendAllTextAsync(this.logFilePath, messageText);
                }
                catch
                {
                    await LogErrorAsync($"Unable to write to log file!", true);
                }

            }
        }
    }
}