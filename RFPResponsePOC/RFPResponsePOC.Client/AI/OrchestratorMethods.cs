using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using OpenAI;
using RFPResponsePOC.Client.Models;
using RFPResponsePOC.Model;
using System;
using System.ClientModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using RFPResponsePOC.Client.Pages;

namespace RFPResponsePOC.AI
{
    public partial class OrchestratorMethods
    {
        public SettingsService SettingsService { get; set; }
        public LogService LogService { get; set; }
        public string Summary { get; set; }

        public List<(string, float)> similarities = new List<(string, float)>();

        // Constructor
        public OrchestratorMethods(SettingsService _SettingsService, LogService _LogService)
        {
            SettingsService = _SettingsService;
            LogService = _LogService;
        }

        // OpenAI Service

        #region public OpenAIClient CreateOpenAIClient()
        public OpenAIClient CreateOpenAIClient()
        {
            SettingsService.LoadSettings();

            string ApiKey = SettingsService.Settings.ApplicationSettings.ApiKey;
            string AIEmbeddingModel = SettingsService.Settings.ApplicationSettings.AIEmbeddingModel;
            string AIModel = SettingsService.Settings.ApplicationSettings.AIModel;

            var options = new OpenAIClientOptions();
            options.NetworkTimeout = TimeSpan.FromSeconds(520);

            var auth = new ApiKeyCredential(ApiKey);

            OpenAIClient api;

            api = new OpenAIClient(auth, options);

            return api;
        }
        #endregion       

        // Memory and Vectors

        #region public async Task<string> GetVectorEmbeddingAsync(string EmbeddingContent, bool Combine)
        public async Task<string> GetVectorEmbeddingAsync(string EmbeddingContent, bool Combine)
        {
            // **** Call OpenAI and get embeddings for the memory text
            // Create an instance of the OpenAI client
            OpenAIClient api = CreateOpenAIClient();

            SettingsService.LoadSettings();

            string ApiKey = SettingsService.Settings.ApplicationSettings.ApiKey;

            // Get embeddings for the text
            var EmbeddingClient = api.GetEmbeddingClient("text-embedding-3-small");

            var embeddings = await EmbeddingClient.GenerateEmbeddingAsync(EmbeddingContent);

            // Get embeddings as an array of floats
            var EmbeddingVectors = embeddings.Value.ToFloats().ToArray();

            // Loop through the embeddings
            List<VectorData> AllVectors = new List<VectorData>();
            for (int i = 0; i < EmbeddingVectors.Length; i++)
            {
                var embeddingVector = new VectorData
                {
                    VectorValue = EmbeddingVectors[i]
                };
                AllVectors.Add(embeddingVector);
            }

            // Convert the floats to a single string
            var VectorsToSave = "[" + string.Join(",", AllVectors.Select(x => x.VectorValue)) + "]";

            if (Combine)
            {
                return EmbeddingContent + "|" + VectorsToSave;
            }
            else
            {
                return VectorsToSave;
            }
        }
        #endregion

        #region public async Task<float[]> GetVectorEmbeddingAsFloats(string EmbeddingContent)
        public async Task<float[]> GetVectorEmbeddingAsFloats(string EmbeddingContent)
        {
            // **** Call OpenAI and get embeddings for the memory text
            // Create an instance of the OpenAI client
            OpenAIClient api = CreateOpenAIClient();

            SettingsService.LoadSettings();

            string ApiKey = SettingsService.Settings.ApplicationSettings.ApiKey;

            // Get embeddings for the text
            var EmbeddingClient = api.GetEmbeddingClient("text-embedding-3-small");

            var embeddings = await EmbeddingClient.GenerateEmbeddingAsync(EmbeddingContent);

            // Get embeddings as an array of floats
            var EmbeddingVectors = embeddings.Value.ToFloats().ToArray();

            return EmbeddingVectors;
        }
        #endregion

        // Utility Methods

        #region public float CosineSimilarity(float[] vector1, float[] vector2)
        public float CosineSimilarity(float[] vector1, float[] vector2)
        {
            // Initialize variables for dot product and
            // magnitudes of the vectors
            float dotProduct = 0;
            float magnitude1 = 0;
            float magnitude2 = 0;

            // Iterate through the vectors and calculate
            // the dot product and magnitudes
            for (int i = 0; i < vector1?.Length; i++)
            {
                // Calculate dot product
                dotProduct += vector1[i] * vector2[i];

                // Calculate squared magnitude of vector1
                magnitude1 += vector1[i] * vector1[i];

                // Calculate squared magnitude of vector2
                magnitude2 += vector2[i] * vector2[i];
            }

            // Take the square root of the squared magnitudes
            // to obtain actual magnitudes
            magnitude1 = (float)Math.Sqrt(magnitude1);
            magnitude2 = (float)Math.Sqrt(magnitude2);

            // Calculate and return cosine similarity by dividing
            // dot product by the product of magnitudes
            return dotProduct / (magnitude1 * magnitude2);
        }
        #endregion

        #region private string CombineAndSortLists(string paramExistingList, string paramNewList)
        private string CombineAndSortLists(string paramExistingList, string paramNewList)
        {
            // Split the lists into an arrays
            string[] ExistingListArray = paramExistingList.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] NewListArray = paramNewList.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Combine the lists
            string[] CombinedListArray = ExistingListArray.Concat(NewListArray).ToArray();

            // Remove duplicates
            CombinedListArray = CombinedListArray.Distinct().ToArray();

            // Sort the array
            Array.Sort(CombinedListArray);

            // Combine the array into a string
            string CombinedList = string.Join("\n", CombinedListArray);

            return CombinedList;
        }
        #endregion

        #region public static string TrimToMaxWords(string input, int maxWords = 500)
        public static string TrimToMaxWords(string input, int maxWords = 500)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string[] words = input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length <= maxWords)
                return input;

            return string.Join(" ", words.Take(maxWords));
        }
        #endregion

        #region public bool IsValidFolderName(string folderName)
        public bool IsValidFolderName(string folderName)
        {
            string invalidChars = new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars());
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(invalidChars) + "]");
            if (containsABadCharacter.IsMatch(folderName))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region public static string TrimInnerSpaces(string input)
        public static string TrimInnerSpaces(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return Regex.Replace(input, @"\s{2,}", " ");
        }
        #endregion

        #region public string SanitizeFileName(string input)
        private static readonly char[] InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
        private static readonly string[] ReservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

        public string SanitizeFileName(string input)
        {
            // Strip out invalid characters
            string sanitized = new string(input.Where(ch => !InvalidFileNameChars.Contains(ch)).ToArray());

            // Remove the | character
            sanitized = sanitized.Replace("|", "");

            return sanitized;
        }
        #endregion

        #region public async Task AnswerQuestionsFromKnowledgebase(QuestionResponse question, string basePath)
        public async Task AnswerQuestionsFromKnowledgebase(QuestionResponse question, string basePath)
        {
            if (question == null)
                return;

            try
            {
                var kbPath = $"{basePath}//knowledgebase.json";
                if (!File.Exists(kbPath))
                    return;

                var kbJson = await File.ReadAllTextAsync(kbPath);
                var entries = JsonConvert.DeserializeObject<List<KnowledgeChunk>>(kbJson) ?? new List<KnowledgeChunk>();
                if (!entries.Any())
                    return;

                var questionEmbedding = await GetVectorEmbeddingAsFloats(question.Question);

                var scores = new List<(KnowledgeChunk chunk, float score)>();
                foreach (var entry in entries)
                {
                    var embeddingText = entry.Embedding ?? string.Empty;
                    if (embeddingText.Contains("|"))
                        embeddingText = embeddingText[(embeddingText.IndexOf('|') + 1)..];

                    float[] entryEmbedding;
                    try
                    {
                        entryEmbedding = JsonConvert.DeserializeObject<float[]>(embeddingText);
                    }
                    catch
                    {
                        var parts = embeddingText.Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries);
                        entryEmbedding = parts.Select(p => float.Parse(p.Trim())).ToArray();
                    }

                    var score = CosineSimilarity(questionEmbedding, entryEmbedding);
                    scores.Add((entry, score));
                }

                var topChunks = scores
                    .OrderByDescending(s => s.score)
                    .Take(20)
                    .Select(s => s.chunk.Content)
                    .ToList();

                if (!topChunks.Any())
                    return;

                var promptPath = $"{basePath}//wwwroot//Prompts//AnswerQuestion.prompt";
                var prompt = await File.ReadAllTextAsync(promptPath);
                var knowledgeText = string.Join("\n", topChunks);
                prompt = prompt.Replace("{{Question}}", question.Question)
                               .Replace("{{Knowledgebase}}", knowledgeText);

                var aiResponse = await CallOpenAIAsync(SettingsService, prompt);
                if (!string.IsNullOrWhiteSpace(aiResponse.Error) || string.IsNullOrWhiteSpace(aiResponse.Response))
                    return;

                try
                {
                    var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(aiResponse.Response);
                    if (obj != null && obj.TryGetValue("answer", out var answer) && !string.IsNullOrWhiteSpace(answer))
                    {
                        question.Response = answer;
                    }
                }
                catch
                {
                    // Ignore parsing errors
                }
            }
            catch (Exception ex)
            {
                await LogService.WriteToLogAsync($"[{DateTime.Now}] ERROR: AnswerQuestionsFromKnowledgebase - {ex.Message}");
            }
        }
        #endregion

        #region public static List<string> ParseStringToList(string input)
        public static List<string> ParseStringToList(string input)
        {
            // Remove the brackets and split the string by comma
            string[] items = Regex.Replace(input, @"[\[\]]", "").Split(',');

            // Convert the array to a List<string> and return
            return new List<string>(items);
        }
        #endregion

        #region public class ReadTextEventArgs : EventArgs
        public class ReadTextEventArgs : EventArgs
        {
            public string Message { get; set; }
            public int DisplayLength { get; set; }

            public ReadTextEventArgs(string message, int display_length)
            {
                Message = message;
                DisplayLength = display_length;
            }
        }
        #endregion

        #region public static string ExtractJsonFromResponse(string input)
        public static string ExtractJsonFromResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // First try to find JSON array
            int arrayStartIndex = input.IndexOf('[');
            int arrayEndIndex = input.LastIndexOf(']');

            // If we found a valid JSON array, return it
            if (arrayStartIndex != -1 && arrayEndIndex != -1 && arrayEndIndex > arrayStartIndex)
            {
                return input.Substring(arrayStartIndex, arrayEndIndex - arrayStartIndex + 1);
            }

            // Fallback to JSON object extraction
            int startIndex = input.IndexOf('{');
            int endIndex = input.LastIndexOf('}');

            // Validate positions
            if (startIndex == -1 || endIndex == -1 || endIndex < startIndex)
                return string.Empty;

            // Extract and return the substring that should represent valid JSON
            return input.Substring(startIndex, endIndex - startIndex + 1);
        } 
        #endregion
    }
}