using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Configuration;

namespace AzureOCRConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Settings settings = GetSettings();

            AzureKeyCredential cred = new AzureKeyCredential(settings.Key);
            DocumentAnalysisClient client = new DocumentAnalysisClient(
                new Uri(settings.Endpoint), cred);

            string imagesHome = "T:\\Temp\\CopyOf_2019_CellarFloodBooks";
            string fileName = Path.Combine(
                imagesHome, 
                "prep_images_output\\grayscale",
                "IMG_0641-grayscale.jpg");

            var program = new Program();

            await program.ProcessImage(settings, fileName, client);
        }

        private async Task ProcessImage(Settings settings, string fileName, DocumentAnalysisClient client)
        {
            Console.WriteLine($"Reading '{fileName}'");

            //using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using FileStream stream = File.OpenRead(fileName);

            AnalyzeDocumentOperation op = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", stream);

            //string dirName = Path.GetDirectoryName(fileName) ?? Directory.GetCurrentDirectory();
            string dirName = Path.GetDirectoryName(fileName) ?? AppDomain.CurrentDomain.BaseDirectory;

            string outFileName = Path.Combine(dirName, Path.ChangeExtension(Path.GetFileName(fileName), ".ocr.txt"));

            Console.WriteLine($"Writing '{outFileName}'");

            using (FileStream outStream = File.Create(outFileName))
            using (TextWriter writer = new StreamWriter(outStream))
            {
                AnalyzeResult result = op.Value;
                foreach (DocumentPage page in result.Pages)
                {
                    for (int i = 0; i < page.Lines.Count; i++)
                    {
                        DocumentLine line = page.Lines[i];
                        Console.WriteLine($"  Line {i}: '{line.Content}'");
                        writer.WriteLine(line.Content);
                    }
                }
            }

        }

        private static Settings GetSettings()
        {
            string configFile = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "KeepLocal", "azure-ocr-settings.json");

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