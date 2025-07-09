using Blazored.LocalStorage;
using Radzen;
using RFPResponsePOC.Client.Pages;
using RFPResponsePOC.Components;
using RFPResponsePOC.Model;
using RFPResponsePOC.Models;

namespace RFPResponsePOC;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        // Local Storage
        builder.Services.AddBlazoredLocalStorage();

        // Add services to the container.
        AppMetadata appMetadata = new AppMetadata() { Version = "01.00.00" };
        builder.Services.AddSingleton(appMetadata);

        builder.Services.AddScoped<LogService>();
        builder.Services.AddScoped<SettingsService>();
        builder.Services.AddScoped<DatabaseService>();

        builder.Services.AddRadzenComponents();

        builder.Services.AddHttpClient();
        builder.Services.AddScoped<HttpClient>();

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

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();
    }
}
