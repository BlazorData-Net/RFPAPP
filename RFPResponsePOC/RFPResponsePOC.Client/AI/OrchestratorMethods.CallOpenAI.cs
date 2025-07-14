using Microsoft.Extensions.AI;
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
            var chatClient = CreateAIChatClient(AIType, AIModel, ApiKey, Endpoint, AIEmbeddingModel);
            string SystemMessage = "Please return the following as json: \"This is successful\" in this format {\r\n  'message': message\r\n}";
            var response = await chatClient.CompleteAsync(SystemMessage);

            if (response.Choices.Count == 0)
            {
                return false;
            }

            // Pass though ExtractJsonFromResponse
            var json = ExtractJsonFromResponse(response.Choices.FirstOrDefault().Text);

            return true;
        }
        #endregion

        #region public async Task<AIResponse> CallOpenAIAsync(SettingsService objSettings, string ApiKey, string AIQuery)
        public async Task<AIResponse> CallOpenAIAsync(SettingsService objSettings, string ApiKey, string AIQuery)
        {
            try
            {
                // Create the AI chat client using the provided settings
                var chatClient = CreateAIChatClient(
                    SettingsService.Settings.ApplicationSettings.AIType,
                    SettingsService.Settings.ApplicationSettings.AIModel,
                    ApiKey,
                    SettingsService.Settings.ApplicationSettings.Endpoint,
                    SettingsService.Settings.ApplicationSettings.AIEmbeddingModel);

                // Send the system message to the AI chat client
                var response = await chatClient.CompleteAsync(AIQuery);

                // Check if the response contains any choices
                if (response.Choices == null || response.Choices.Count == 0)
                {
                    // Optionally, log the absence of choices or handle it as needed
                    return new AIResponse() { Response = "", Error = "No choices returned in the AI response." };
                }

                // Extract the text from the first choice
                string jsonResponse = response.Choices[0].Text.Trim();

                jsonResponse = ExtractJsonFromResponse(jsonResponse);

                try
                {
                    var parsedResponse = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);

                    if (parsedResponse != null && !string.IsNullOrEmpty(parsedResponse.Response))
                    {
                        return parsedResponse;
                    }
                    else
                    {
                        return new AIResponse() { Response = "", Error = "Parsed response is null or missing the 'Response' field." };
                    }
                }
                catch (JsonException jsonEx)
                {
                    // Handle JSON parsing errors
                    await LogService.WriteToLogAsync($"Error parsing JSON response: {jsonEx.Message}");
                    return new AIResponse() { Response = "", Error = $"Error parsing JSON response: {jsonEx.Message}" };
                }
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
            try
            {
                var chatClient = CreateAIChatClient(
                    objSettings.Settings.ApplicationSettings.AIType,
                    objSettings.Settings.ApplicationSettings.AIModel,
                    apiKey,
                    objSettings.Settings.ApplicationSettings.Endpoint,
                    objSettings.Settings.ApplicationSettings.AIEmbeddingModel);

                // prepare file content
                string fileContent;
                try { fileContent = Encoding.UTF8.GetString(fileBytes); }
                catch { fileContent = Convert.ToBase64String(fileBytes); }

                // build messages
                var messages = new List<Microsoft.Extensions.AI.ChatMessage>
                {
                    // 1) Strict JSON instruction
                    new Microsoft.Extensions.AI.ChatMessage(
                        Microsoft.Extensions.AI.ChatRole.System,
                        "When you reply, output strictly a JSON object with a single property \"Response\" " +
                        "containing your answer. Do NOT include any other keys or extra text."),

                    // 2) User prompt
                    new Microsoft.Extensions.AI.ChatMessage(
                        Microsoft.Extensions.AI.ChatRole.User,
                        prompt),

                    // 3) File payload
                    new Microsoft.Extensions.AI.ChatMessage(
                        Microsoft.Extensions.AI.ChatRole.User,
                        $"Here is the file content (Base64 if binary):\n```\n{fileContent}\n```")
                };

                // send
                Microsoft.Extensions.AI.ChatCompletion completion =
                    await chatClient.CompleteAsync(messages);

                // log raw for debugging
                string raw = completion.Message.Text?.Trim() ?? "";
                await LogService.WriteToLogAsync($"[DEBUG] Raw AI response: {raw}");

                // extract JSON blob
                string jsonResponse = ExtractJsonFromResponse(raw);

                // parse
                try
                {
                    // Parse into a JObject so we can inspect the "Response" token
                    var root = JObject.Parse(jsonResponse);

                    var token = root["Response"];

                    if (token == null)
                    {
                        return new AIResponse
                        {
                            Response = "",
                            Error = "JSON did not contain a \"Response\" property."
                        };
                    }

                    // If it's already a string, grab it; otherwise re-serialize the object
                    string responseString = token.Type == JTokenType.String
                        ? token.Value<string>()
                        : token.ToString(Formatting.None);

                    return new AIResponse
                    {
                        Response = responseString,
                        Error = null
                    };
                }
                catch (JsonException jex)
                {
                    await LogService.WriteToLogAsync($"Error parsing JSON response: {jex.Message}");
                    return new AIResponse
                    {
                        Response = "",
                        Error = $"Error parsing JSON response: {jex.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                await LogService.WriteToLogAsync($"An error occurred: {ex.Message}");
                return new AIResponse
                {
                    Response = "",
                    Error = $"An error occurred while calling the AI model: {ex.Message}"
                };
            }
        }
        #endregion
    }
}
