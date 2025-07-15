using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Chat;
using RFPResponsePOC.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

        #region public async Task<AIResponse> CallOpenAIFileAsync(SettingsService objSettings, string apiKey, string AIModel, string prompt, byte[] fileBytes)
        public async Task<AIResponse> CallOpenAIFileAsync(
            SettingsService objSettings,
            string apiKey,
            string AIModel,
            string prompt,
            byte[] fileBytes)
        {
            JsonDocument doc;

            try
            {
                string base64 = Convert.ToBase64String(fileBytes);

                var request = new
                {
                    model = AIModel,
                    messages = new object[]
                    {
                        new {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "text", text = prompt },
                                new {
                                    type = "image_url",
                                    image_url = new {
                                        url = $"data:image/png;base64,{base64}"
                                    }
                                }
                            }
                        }
                    },
                    max_completion_tokens = 10000
                };

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"OpenAI error: {response.StatusCode} - {error}");
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                doc = await JsonDocument.ParseAsync(stream);

            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                await LogService.WriteToLogAsync($"An error occurred while calling the AI model: {ex.Message}");
                return new AIResponse() { Response = "", Error = $"An error occurred while processing the file: {ex.Message}" };
            }

            return new AIResponse
            {
                Response = doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString() ?? "No text found.",
                Error = ""
            };
        }
        #endregion
    }
}
