using Microsoft.Extensions.Configuration;

namespace DigitalWellbeing
{
    public class Configuration
    {
        private readonly IConfiguration _configuration;

        public Configuration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string DBSecret
        {
            get => _configuration["DBSecret"]!;
        }

        public string BasePath
        {
            get => _configuration["BasePath"]!;
        }

        public string FilePath
        {
            get => _configuration["FilePath"]!;
        }

        public string LocalConnectionString
        {
            get => _configuration["LocalConnectionString"]!;
        }

    }
}
