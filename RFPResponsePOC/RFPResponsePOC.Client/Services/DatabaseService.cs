using Blazored.LocalStorage;
using Newtonsoft.Json;
using OpenAI.Files;
using OpenAI.Models;

namespace RFPResponsePOC.Model
{
    public class Database
    {
        public Dictionary<string, string> colRFPResponsePOCDatabase { get; set; }
    }

    public class DatabaseService
    {
        // Properties
        public Dictionary<string, string> colRFPResponsePOCDatabase { get; set; }

        private ILocalStorageService localStorage;

        // Constructor
        public DatabaseService(ILocalStorageService LocalStorage)
        {
            localStorage = LocalStorage;
        }

        public async Task LoadDatabaseAsync()
        {
            Database RFPResponsePOCDatabase = await localStorage.GetItemAsync<Database>("RFPResponsePOCDatabase");

            if (RFPResponsePOCDatabase == null)
            {
                // Create a new Database instance
                RFPResponsePOCDatabase = new Database();

                RFPResponsePOCDatabase.colRFPResponsePOCDatabase = new Dictionary<string, string>();

                await localStorage.SetItemAsync("RFPResponsePOCDatabase", RFPResponsePOCDatabase);
            }

            colRFPResponsePOCDatabase = RFPResponsePOCDatabase.colRFPResponsePOCDatabase;
        }

        public async Task SaveDatabaseAsync(Dictionary<string, string> paramColRFPResponsePOCDatabase)
        {
            var RFPResponsePOCDatabase = new Database();

            RFPResponsePOCDatabase.colRFPResponsePOCDatabase = paramColRFPResponsePOCDatabase;

            await localStorage.SetItemAsync("RFPResponsePOCDatabase", RFPResponsePOCDatabase);

            // Update the properties
            colRFPResponsePOCDatabase = paramColRFPResponsePOCDatabase;
        }
    }
}