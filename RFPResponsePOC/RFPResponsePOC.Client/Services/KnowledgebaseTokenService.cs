using Newtonsoft.Json;
using RFPResponseAPP.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RFPResponseAPP.Client.Services
{
    public class KnowledgebaseTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly string _basePath;

        public KnowledgebaseTokenService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _basePath = "/RFPResponseAPP";
        }

        public async Task SaveKnowledgebaseTokensDataAsync(IEnumerable<KnowledgeToken> tokens)
        {
            var json = JsonConvert.SerializeObject(tokens, Formatting.Indented);

            var directory = Path.GetDirectoryName($"{_basePath}/KnowledgebaseToken.json");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync($"{_basePath}/KnowledgebaseToken.json", json);
        }

        public async Task<string> GetKnowledgebaseTokenJsonAsync()
        {
            try
            {
                var path = $"{_basePath}/KnowledgebaseToken.json";
                if (File.Exists(path))
                {
                    return await File.ReadAllTextAsync(path);
                }
                else
                {
                    var defaultJson = await _httpClient.GetStringAsync("DefaultContent/DefaultKnowledgebaseToken.json");
                    if (!string.IsNullOrEmpty(defaultJson))
                    {
                        var directory = Path.GetDirectoryName(path);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        await File.WriteAllTextAsync(path, defaultJson);
                        return defaultJson;
                    }
                    else
                    {
                        Console.WriteLine("Default KnowledgebaseToken file not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading KnowledgebaseToken.json: {ex.Message}");
            }
            return null;
        }
    }
}
