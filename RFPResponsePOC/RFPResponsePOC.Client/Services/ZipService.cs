using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Radzen;
using RFPResponseAPP.Model;
using System.IO.Compression;
using RFPResponseAPP.Client.Pages; // for LoadingDialog component

namespace RFPResponseAPP.Client.Services
{
    public class ZipService
    {
        string BasePath = @"/RFPResponseAPP";
        private readonly IJSRuntime JsRuntime;
        private readonly ILocalStorageService localStorage;
        private readonly SettingsService _SettingsService;
        private readonly LogService LogService;
        private readonly DialogService _DialogService;

        public ZipService() { }

        // Backwards-compatible 4-arg ctor
        public ZipService(IJSRuntime jsRuntime,
                          ILocalStorageService localStorage,
                          SettingsService settingsService,
                          LogService logService) : this(jsRuntime, localStorage, settingsService, logService, null) { }

        public ZipService(IJSRuntime jsRuntime,
                          ILocalStorageService localStorage,
                          SettingsService settingsService,
                          LogService logService,
                          DialogService dialogService)
        {
            JsRuntime = jsRuntime;
            this.localStorage = localStorage;
            _SettingsService = settingsService;
            LogService = logService;
            _DialogService = dialogService;
        }

        public async Task<bool> IsZipFileExistsAsync() => await localStorage.ContainKeyAsync("ZipFiles.zip");

        private void EnsureDirectoryStructure()
        {
            // Ensure the base directory exists
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
        }

        public async Task ZipTheFiles()
        {
            string zipPath = @"/Zip";
            string zipFilePath = @"/Zip/ZipFiles.zip";

            if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
            if (!Directory.Exists(zipPath)) Directory.CreateDirectory(zipPath);
            if (!Directory.Exists(BasePath)) return;

            ZipFile.CreateFromDirectory(BasePath, zipFilePath);
            byte[] exportFileBytes = File.ReadAllBytes(zipFilePath);
            string base64String = Convert.ToBase64String(exportFileBytes);
            await localStorage.SetItemAsync("ZipFiles.zip", base64String);
        }

        public async Task UploadZipFile(byte[] zipFileBytes)
        {
            string base64String = Convert.ToBase64String(zipFileBytes);
            await localStorage.SetItemAsync("ZipFiles.zip", base64String);
            // Don't log here - wait until after extraction to ensure log file exists
        }

        public async Task UnzipFile()
        {
            string extractPath = @"/Zip";
            if (!Directory.Exists(extractPath)) Directory.CreateDirectory(extractPath);

            // Delete existing files in the extract path
            if (Directory.Exists(BasePath))
            {
                Directory.Delete(BasePath, true);
            }

            // Ensure the base directory structure exists
            EnsureDirectoryStructure();

            // Create a new log file if it doesn't exist
            if (!File.Exists(@$"{BasePath}/RFPResponseAPPLog.csv"))
            {
                using (var streamWriter = new StreamWriter(@$"{BasePath}/RFPResponseAPPLog.csv"))
                {
                    streamWriter.WriteLine("Application reset at " + DateTime.Now + " [" + DateTime.Now.Ticks.ToString() + "]");
                }
            }

            // Initialize default settings to ensure RFPResponseAPP.config file is not null
            await _SettingsService.InitializeDefaultSettingsAsync();

            string exportFileString = await localStorage.GetItemAsync<string>("ZipFiles.zip");
            byte[] exportFileBytes = Convert.FromBase64String(exportFileString);
            await File.WriteAllBytesAsync($"{extractPath}/ZipFiles.zip", exportFileBytes);
            ZipFile.ExtractToDirectory($"{extractPath}/ZipFiles.zip", BasePath, true);
            await LogService.WriteToLogAsync("[" + DateTime.Now + "] Saved data extracted.");
        }

        public async Task DownloadZipFile()
        {
            string base64String = await localStorage.GetItemAsync<string>("ZipFiles.zip");

            try { _DialogService?.Close(); } catch { }

            await JsRuntime.InvokeVoidAsync("saveAsFile", "ZipFiles.zip", base64String);
        }

        public void DeleteZipFile()
        {
            localStorage.RemoveItemAsync("ZipFiles.zip");
            string zipFilePath = @"/Zip/ZipFiles.zip";
            if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
            if (Directory.Exists(BasePath)) Directory.Delete(BasePath, true);
        }
    }
}