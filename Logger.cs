namespace DigitalWellbeing
{
    public class Logger
    {
        private readonly Configuration _configuration;
        private readonly Utils _utils;

        public Logger(Configuration configuration, Utils utils)
        {
            _configuration = configuration;
            _utils = utils;
        }

        public void LogIntoFile(string logMessage)
        {
            using (StreamWriter writer = new StreamWriter(_configuration.FilePath, true))
            {
                writer.WriteLine(logMessage);
            }
        }

        public void LogAppUsage(string appName, DateTime start, DateTime end)
        {
            string cleanAppName = _utils.ExtractAppName(appName);
            string logMessage = $"Logged: {cleanAppName} | Duration: {end - start}";
            LogIntoFile(logMessage);
        }

        public async Task ClearLogFile()
        {
            string filePath = _configuration.FilePath;

            if (File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, string.Empty);
            }
            else
            {
                Console.WriteLine("Log file does not exist.");
            }
        }
    }
}
