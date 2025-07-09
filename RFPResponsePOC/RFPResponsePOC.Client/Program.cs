using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using RFPResponsePOC.Model;
using RFPResponsePOC.Models;

namespace RFPResponsePOC.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddRadzenComponents();

        // Local Storage
        builder.Services.AddBlazoredLocalStorage();

        // Add services to the container.
        AppMetadata appMetadata = new AppMetadata() { Version = "01.00.00" };
        builder.Services.AddSingleton(appMetadata);

        builder.Services.AddScoped<LogService>();
        builder.Services.AddScoped<SettingsService>();
        builder.Services.AddScoped<DatabaseService>();

        // Set the base address for the application
        var http = new HttpClient()
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        };

        // Load Default files
        var folderPath = "";
        var filePath = "";

        // RFPResponsePOC Directory
        folderPath = $"/RFPResponsePOC";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // RFPResponsePOCLog.csv
        filePath = Path.Combine(folderPath, "RFPResponsePOCLog.csv");

        if (!File.Exists(filePath))
        {
            using (var streamWriter = new StreamWriter(filePath))
            {
                streamWriter.WriteLine("Application started at " + DateTime.Now + " [" + DateTime.Now.Ticks.ToString() + "]");
            }
        }
        else
        {
            // File already exists
            string[] RFPResponsePOCLog;

            // Open the file to get existing content
            using (var file = new System.IO.StreamReader(filePath))
            {
                RFPResponsePOCLog = file.ReadToEnd().Split('\n');

                if (RFPResponsePOCLog[RFPResponsePOCLog.Length - 1].Trim() == "")
                {
                    RFPResponsePOCLog = RFPResponsePOCLog.Take(RFPResponsePOCLog.Length - 1).ToArray();
                }
            }

            // Append the text to csv file
            using (var streamWriter = new StreamWriter(filePath))
            {
                streamWriter.WriteLine(string.Join("\n", "Application started at " + DateTime.Now));
                streamWriter.WriteLine(string.Join("\n", RFPResponsePOCLog));
            }
        }

        await builder.Build().RunAsync();
    }
}