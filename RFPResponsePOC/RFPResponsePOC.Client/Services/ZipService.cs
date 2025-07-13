using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Radzen;
using RFPResponsePOC.Model;
using System.IO.Compression;

namespace RFPResponsePOC.Client.Services
{
    public class ZipService
    {
        string BasePath = @"/RFPResponsePOC";
        private readonly IJSRuntime JsRuntime;
        private readonly ILocalStorageService localStorage;
        private readonly SettingsService _SettingsService;
        private readonly LogService LogService;

        public ZipService()
        {
        }

        public ZipService(IJSRuntime jsRuntime, ILocalStorageService localStorage, SettingsService settingsService, LogService logService)
        {
            JsRuntime = jsRuntime;
            this.localStorage = localStorage;
            _SettingsService = settingsService;
            LogService = logService;
        }

        public async Task<bool> IsZipFileExistsAsync()
        {
            return await localStorage.ContainKeyAsync("ZipFiles.zip");
        }

        public async Task ZipTheFiles()
        {
            string zipPath = @"/Zip";
            string zipFilePath = @"/Zip/ZipFiles.zip";

            // If zipFilePath exists, delete it
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            // Create the directory if it doesn't exist
            if (!Directory.Exists(zipPath))
            {
                Directory.CreateDirectory(zipPath);
            }

            // If BasePath is not a directory, return
            if (!Directory.Exists(BasePath))
            {
                return;
            }

            // Create a zip file from the directory
            ZipFile.CreateFromDirectory(BasePath, zipFilePath);

            // Read the Zip file into a byte array
            byte[] exportFileBytes = File.ReadAllBytes(zipFilePath);

            // Convert byte array to Base64 string
            string base64String = Convert.ToBase64String(exportFileBytes);

            // Store base64String in the browser's local storage
            await localStorage.SetItemAsync("ZipFiles.zip", base64String);
        }

        public async Task UnzipFile()
        {
            string extractPath = @"/Zip";

            // If the extract directory does not exist, create it
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            // Get exportFileString from the browser's local storage
            string exportFileString = await localStorage.GetItemAsync<string>("ZipFiles.zip");

            // Convert the Base64 string to a byte array
            byte[] exportFileBytes = Convert.FromBase64String(exportFileString);

            // Write the byte array to a file
            await File.WriteAllBytesAsync($"{extractPath}/ZipFiles.zip", exportFileBytes);

            // Extract the zip file
            ZipFile.ExtractToDirectory($"{extractPath}/ZipFiles.zip", BasePath, true);

            await LogService.WriteToLogAsync("Saved data extracted at " + DateTime.Now + " [" + DateTime.Now.Ticks.ToString() + "]");
        }

        public async Task DownloadZipFile()
        {
            // Get exportFileString from the browser's local storage
            string base64String = await localStorage.GetItemAsync<string>("ZipFiles.zip");

            // Download the zip file
            await JsRuntime.InvokeVoidAsync("saveAsFile", "ZipFiles.zip", base64String);
        }

        public void DeleteZipFile()
        {
            // Remove the zip file from the browser's local storage
            localStorage.RemoveItemAsync("ZipFiles.zip");

            // Delete the zip file from the server
            string zipFilePath = @"/Zip/ZipFiles.zip";
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            // Delete the extracted files
            if (Directory.Exists(BasePath))
            {
                Directory.Delete(BasePath, true);
            }
        }
    }
}