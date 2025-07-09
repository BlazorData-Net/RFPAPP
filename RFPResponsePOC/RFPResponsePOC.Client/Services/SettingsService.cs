using Blazored.LocalStorage;
using Newtonsoft.Json;
using OpenAI.Files;
using System.Text.Json.Serialization;

namespace RFPResponsePOC.Model
{
    public class Settings
    {
        public string Organization { get; set; }
        public string ApiKey { get; set; }
        public string AIModel { get; set; }
        public string GUID { get; set; }
        public string AIType { get; set; }
        public string Endpoint { get; set; }
        public string AIEmbeddingModel { get; set; }
        public string ApiVersion { get; set; }
    }

    public class SettingsService
    {
        // Properties

        public string Organization { get; set; }
        public string ApiKey { get; set; }
        public string AIModel { get; set; }
        public string GUID { get; set; }
        public string AIType { get; set; }
        public string Endpoint { get; set; }
        public string AIEmbeddingModel { get; set; }
        public string ApiVersion { get; set; }

        private ILocalStorageService localStorage;

        // Constructor
        public SettingsService(ILocalStorageService LocalStorage)
        {
            localStorage = LocalStorage;
        }

        public async Task LoadSettingsAsync()
        {
            Settings RFPResponsePOCSettings = await localStorage.GetItemAsync<Settings>("RFPResponsePOCSettings");

            if (RFPResponsePOCSettings == null)
            {
                // Create a new instance of the SettingsService
                RFPResponsePOCSettings = new Settings();

                RFPResponsePOCSettings.Organization = "";
                RFPResponsePOCSettings.ApiKey = "";
                RFPResponsePOCSettings.AIModel = "gpt-4o";
                RFPResponsePOCSettings.GUID = Guid.NewGuid().ToString();
                RFPResponsePOCSettings.AIType = "";
                RFPResponsePOCSettings.Endpoint = "";
                RFPResponsePOCSettings.ApiVersion = "";
                RFPResponsePOCSettings.AIEmbeddingModel = "";                

                await localStorage.SetItemAsync("RFPResponsePOCSettings", RFPResponsePOCSettings);
            }
            else
            {
                // Create GUID if it is blank
                if (string.IsNullOrEmpty(RFPResponsePOCSettings.GUID))
                {
                    RFPResponsePOCSettings.GUID = Guid.NewGuid().ToString();
                    await localStorage.SetItemAsync("RFPResponsePOCSettings", RFPResponsePOCSettings);
                }

                // Set if AIType is blank
                if (RFPResponsePOCSettings.AIType == null || RFPResponsePOCSettings.AIType == "")
                {
                    RFPResponsePOCSettings.AIType = "OpenAI";
                    await localStorage.SetItemAsync("RFPResponsePOCSettings", RFPResponsePOCSettings);
                }
            }

            Organization = RFPResponsePOCSettings.Organization;
            ApiKey = RFPResponsePOCSettings.ApiKey;
            AIModel = RFPResponsePOCSettings.AIModel;
            GUID = RFPResponsePOCSettings.GUID;
            AIType = RFPResponsePOCSettings.AIType;
            Endpoint = RFPResponsePOCSettings.Endpoint;
            ApiVersion = RFPResponsePOCSettings.ApiVersion;
            AIEmbeddingModel = RFPResponsePOCSettings.AIEmbeddingModel;
        }

        public async Task SaveSettingsAsync(string paramOrganization, string paramApiKey, string paramAIModel, string paramAIType, string paramGUID, string paramEndpoint, string paramApiVersion, string paramAIEmbeddingModel)
        {
            var RFPResponsePOCSettings = new Settings();

            RFPResponsePOCSettings.Organization = paramOrganization;
            RFPResponsePOCSettings.ApiKey = paramApiKey;
            RFPResponsePOCSettings.AIModel = paramAIModel;
            RFPResponsePOCSettings.GUID = paramGUID;
            RFPResponsePOCSettings.AIType = paramAIType;
            RFPResponsePOCSettings.Endpoint = paramEndpoint;
            RFPResponsePOCSettings.ApiVersion = paramApiVersion;
            RFPResponsePOCSettings.AIEmbeddingModel = paramAIEmbeddingModel;

            await localStorage.SetItemAsync("RFPResponsePOCSettings", RFPResponsePOCSettings);

            // Update the properties
            Organization = paramOrganization;
            ApiKey = paramApiKey;
            AIModel = paramAIModel;
            AIType = paramAIType;
            GUID = paramGUID;
            Endpoint = paramEndpoint;
            ApiVersion = paramApiVersion;
            AIEmbeddingModel = paramAIEmbeddingModel;
        }
    }
}