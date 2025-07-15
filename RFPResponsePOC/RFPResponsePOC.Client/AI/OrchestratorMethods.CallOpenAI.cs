using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Chat;
using RFPResponsePOC.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFPResponsePOC.AI
{
    public class AIResponse
    {
        [JsonProperty("response")]
        public string Response { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
    }
    public partial class OrchestratorMethods
    {
        #region public async Task<bool> TestAccessAsync(string AIType, string AIModel, string ApiKey, string Endpoint, string AIEmbeddingModel)
        public async Task<bool> TestAccessAsync(string AIType, string AIModel, string ApiKey, string Endpoint, string AIEmbeddingModel)
        {
            var chatClient = new OpenAIClient(ApiKey);


            // Pass though ExtractJsonFromResponse
            var json = ExtractJsonFromResponse("response.Choices.FirstOrDefault().Text");

            return true;
        }
        #endregion

        #region public async Task<AIResponse> CallOpenAIAsync(SettingsService objSettings, string ApiKey, string AIQuery)
        public async Task<AIResponse> CallOpenAIAsync(SettingsService objSettings, string ApiKey, string AIQuery)
        {
            try
            {
                var chatClient = new OpenAIClient(ApiKey);

                return new AIResponse
                {
                    Response = "response.Text",
                    Error = null
                };
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                await LogService.WriteToLogAsync($"An error occurred while calling the AI model: {ex.Message}");
                return new AIResponse() { Response = "", Error = $"An error occurred while testing access: {ex.Message}" };
            }
        }
        #endregion

        #region public async Task<AIResponse> CallOpenAIFileAsync(SettingsService objSettings, string apiKey, prompt, byte[] fileBytes)
        public async Task<AIResponse> CallOpenAIFileAsync(
            SettingsService objSettings,
            string apiKey,
            string prompt,
            byte[] fileBytes)
        {
            var chatClient = new OpenAIClient(apiKey);
    
            return new AIResponse
            {
                Response = "response.Text",
                Error = null
            };
        }
        #endregion
    }
}
