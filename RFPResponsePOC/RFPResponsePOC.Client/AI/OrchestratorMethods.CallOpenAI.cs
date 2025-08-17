using Azure;
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
        #region public async Task<bool> TestAccessAsync(string apiKey, string model)
        public async Task<bool> TestAccessAsync(string apiKey, string model)
        {
            var client = new ChatClient(model, apiKey);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant that returns only json."),
                new UserChatMessage("Say Hello!")
            };

            var response = await client.CompleteChatAsync(messages);
            var reply = response.Value.Content.FirstOrDefault()?.Text;

            var json = ExtractJsonFromResponse(reply);

            return !string.IsNullOrWhiteSpace(json);
        }
        #endregion

        #region public async Task<AIResponse> CallOpenAIAsync(SettingsService objSettings, string AIQuery)
        public async Task<AIResponse> CallOpenAIAsync(SettingsService objSettings, string AIQuery)
        {
            try
            {
                var client = new ChatClient(objSettings.Settings.ApplicationSettings.AIModel, objSettings.Settings.ApplicationSettings.ApiKey);

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("Please only respond with json. Do not output anything else."),
                    new UserChatMessage(AIQuery)
                };

                var response = await client.CompleteChatAsync(messages);
                var reply = response.Value.Content.FirstOrDefault()?.Text;

                await LogService.WriteToLogAsync(
                    $"CallOpenAIAsync: AI model: {objSettings.Settings.ApplicationSettings.AIModel} Reply: {reply}");

                var json = ExtractJsonFromResponse(reply);

                await LogService.WriteToLogAsync(
                    $"CallOpenAIAsync: AI model: {objSettings.Settings.ApplicationSettings.AIModel} JSON: {json}");

                var usage = response.Value.Usage;
                await LogService.WriteToLogAsync(
                    $"Tokens - Input: {usage.InputTokenCount}, Output: {usage.OutputTokenCount}");

                return new AIResponse
                {
                    Response = json,
                    Error = ""
                };
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                await LogService.WriteToLogAsync($"An error occurred: {ex.Message}");
                return new AIResponse() { Response = "", Error = $"An error occurred: {ex.Message}" };
            }
        }
        #endregion

        #region public async Task<AIResponse> CallOpenAIFileAsync(SettingsService objSettings, string prompt, byte[] fileBytes)
        public async Task<AIResponse> CallOpenAIFileAsync(SettingsService objSettings, string prompt, byte[] fileBytes)
        {
            JsonDocument doc;

            try
            {
                string base64 = Convert.ToBase64String(fileBytes);

                var request = new
                {
                    model = objSettings.Settings.ApplicationSettings.AIModel,
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
                // Increase timeout to 10 minutes for AI processing (OCR and large image processing can take time)
                client.Timeout = TimeSpan.FromMinutes(10);
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", objSettings.Settings.ApplicationSettings.ApiKey);

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

            var Response = doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString() ?? "No text found.";
            var usageElement = doc.RootElement.GetProperty("usage");
            var promptTokens = usageElement.GetProperty("prompt_tokens").GetInt32();
            var completionTokens = usageElement.GetProperty("completion_tokens").GetInt32();

            await LogService.WriteToLogAsync(
                $"CallOpenAIFileAsync: AI model: {objSettings.Settings.ApplicationSettings.AIModel} Response: {Response}");

            await LogService.WriteToLogAsync(
                $"Tokens - Input: {promptTokens}, Output: {completionTokens}");

            return new AIResponse
            {
                Response = Response,
                Error = ""
            };
        }
        #endregion
    }
}
