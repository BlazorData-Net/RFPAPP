using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RFPResponseAPP.AI;
using RFPResponseAPP.Client.Models;
using RFPResponseAPP.Model;

namespace RFPResponseAPP.Client.Services
{
    public class QuestionProcessing
    {
        private readonly HttpClient _http;
        private readonly SettingsService _settingsService;
        private readonly LogService _logService;

        public QuestionProcessing(HttpClient http, SettingsService settingsService, LogService logService)
        {
            _http = http;
            _settingsService = settingsService;
            _logService = logService;
        }

        public async Task<List<QuestionResponse>> IdentifyQuestions(string rfpText)
        {
            var identifiedQuestions = new List<QuestionResponse>();

            try
            {
                var prompt = await _http.GetStringAsync("Prompts/IdentifyQuestions.prompt");
                prompt = prompt.Replace("{{RFPText}}", rfpText ?? string.Empty);

                await _logService.WriteToLogAsync($"[{DateTime.Now}] IdentifyQuestions: Sending prompt to AI...");

                var objOrchestratorMethods = new OrchestratorMethods(_settingsService, _logService);
                var settingsService = new SettingsService();

                var response = await objOrchestratorMethods.CallOpenAIAsync(settingsService, prompt);

                await _logService.WriteToLogAsync($"[{DateTime.Now}] IdentifyQuestions: AI response length: {response.Response?.Length ?? 0}");

                if (string.IsNullOrWhiteSpace(response.Error))
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] IdentifyQuestions: Attempting to deserialize JSON response");
                    identifiedQuestions = JsonConvert.DeserializeObject<List<QuestionResponse>>(response.Response) ?? new List<QuestionResponse>();
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] IdentifyQuestions: Deserialized {identifiedQuestions.Count} questions");
                }
                else
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: IdentifyQuestions failed - {response.Error}");
                }
            }
            catch (Exception ex)
            {
                await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in IdentifyQuestions - {ex.Message}");
            }

            return identifiedQuestions;
        }
    }
}
