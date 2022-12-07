using Microsoft.Extensions.Configuration;

namespace AzureOCRConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Settings settings = GetSettings();


            Console.WriteLine("Hello, World!");
        }
        private static Settings GetSettings()
        {
            string configFile = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "KeepLocal", "cogfunc-config.json");

            if (!File.Exists(configFile))
            {
                Console.WriteLine($"ERROR: Cannot find {configFile}");
                Environment.Exit(1);
            }

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .AddEnvironmentVariables()
                .Build();

            Settings? settings = config.GetRequiredSection("Settings").Get<Settings>();
            if (settings is null)
            {
                Console.WriteLine($"ERROR: Cannot get Settings from {configFile}");
                Environment.Exit(1);
            }

            if (settings.Key is null)
            {
                Console.WriteLine("ERROR: Cannot get 'Key' setting.");
                Environment.Exit(1);
            }

            if (settings.Endpoint is null)
            {
                Console.WriteLine("ERROR: Cannot get 'Endpoint' setting.");
                Environment.Exit(1);
            }

            return settings;
        }
    }
}