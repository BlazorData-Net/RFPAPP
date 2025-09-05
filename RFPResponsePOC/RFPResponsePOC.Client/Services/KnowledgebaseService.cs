using Newtonsoft.Json;
using RFPResponseAPP.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RFPResponseAPP.Client.Services
{
    public class KnowledgebaseService
    {
        private readonly HttpClient _httpClient;
        private readonly string _basePath;

        public KnowledgebaseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _basePath = "/RFPResponseAPP";
        }

        public async Task SaveKnowledgebaseDataAsync(IEnumerable<KnowledgeChunk> entries)
        {
            var json = JsonConvert.SerializeObject(entries, Formatting.Indented);
            
            // Ensure the directory exists
            var directory = Path.GetDirectoryName($"{_basePath}/knowledgebase.json");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            await File.WriteAllTextAsync($"{_basePath}/knowledgebase.json", json);
        }

        public async Task<string> GetKnowledgebaseJsonAsync()
        {
            try
            {
                var path = $"{_basePath}/knowledgebase.json";
                if (File.Exists(path))
                {
                    return await File.ReadAllTextAsync(path);
                }
                else
                {
                    // Create the file from the data at: DefaultContent\DefaultKnowledgebase.json
                    var defaultJson = await _httpClient.GetStringAsync("DefaultContent/DefaultKnowledgebase.json");
                    if (!string.IsNullOrEmpty(defaultJson))
                    {
                        // Ensure the directory exists before writing
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
                        Console.WriteLine("Default knowledgebase file not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading knowledgebase.json: {ex.Message}");
            }
            return null;
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
