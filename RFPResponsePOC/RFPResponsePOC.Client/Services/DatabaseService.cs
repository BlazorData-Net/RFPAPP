using Blazored.LocalStorage;
using Newtonsoft.Json;
using OpenAI.Files;
using OpenAI.Models;

namespace RFPResponseAPP.Model
{
    public class Database
    {
        public Dictionary<string, string> colRFPResponseAPPDatabase { get; set; }
    }

    public class DatabaseService
    {
        // Properties
        public Dictionary<string, string> colRFPResponseAPPDatabase { get; set; }

        private ILocalStorageService localStorage;

        // Constructor
        public DatabaseService(ILocalStorageService LocalStorage)
        {
            localStorage = LocalStorage;
        }

        public async Task LoadDatabaseAsync()
        {
            Database RFPResponseAPPDatabase = await localStorage.GetItemAsync<Database>("RFPResponseAPPDatabase");

            if (RFPResponseAPPDatabase == null)
            {
                // Create a new Database instance
                RFPResponseAPPDatabase = new Database();

                RFPResponseAPPDatabase.colRFPResponseAPPDatabase = new Dictionary<string, string>();

                await localStorage.SetItemAsync("RFPResponseAPPDatabase", RFPResponseAPPDatabase);
            }

            colRFPResponseAPPDatabase = RFPResponseAPPDatabase.colRFPResponseAPPDatabase;
        }

        public async Task SaveDatabaseAsync(Dictionary<string, string> paramColRFPResponseAPPDatabase)
        {
            var RFPResponseAPPDatabase = new Database();

            RFPResponseAPPDatabase.colRFPResponseAPPDatabase = paramColRFPResponseAPPDatabase;

            await localStorage.SetItemAsync("RFPResponseAPPDatabase", RFPResponseAPPDatabase);

            // Update the properties
            colRFPResponseAPPDatabase = paramColRFPResponseAPPDatabase;
        }
    }
}