using Newtonsoft.Json;
using RFPResponsePOC.Client.Models;
using System.Collections.Generic;
using System.IO;
using System;

namespace RFPResponsePOC.Client.Services
{
    public class KnowledgebaseService
    {
        private readonly string _basePath;

        public KnowledgebaseService()
        {
            _basePath = "/RFPResponsePOC";
        }

        public KnowledgebaseService(string basePath)
        {
            _basePath = basePath;
        }

        public async Task SaveKnowledgebaseDataAsync(IEnumerable<KnowledgeChunk> entries)
        {
            var json = JsonConvert.SerializeObject(entries, Formatting.Indented);
            await File.WriteAllTextAsync($"{_basePath}//knowledgebase.json", json);
        }

        public async Task<string> GetKnowledgebaseJsonAsync()
        {
            try
            {
                var path = $"{_basePath}//knowledgebase.json";
                if (File.Exists(path))
                {
                    return await File.ReadAllTextAsync(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading knowledgebase.json: {ex.Message}");
            }
            return null;
        }
    }
}
