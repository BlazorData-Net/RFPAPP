using Newtonsoft.Json;
using OpenAI.Files;

namespace RFPResponseAPP.Model
{
    public class LogService
    {
        // Properties
        public string[] RFPResponseAPPLog { get; set; }

        public async Task WriteToLogAsync(string LogText)
        {
            await Task.Run(() =>
            {
                // Open the file to get existing content
                var RFPResponseAPPLogPath =
                    $"/RFPResponseAPP/RFPResponseAPPLog.csv";

                // Ensure the directory exists
                var directory = Path.GetDirectoryName(RFPResponseAPPLogPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Check if file exists, if not create it
                if (!File.Exists(RFPResponseAPPLogPath))
                {
                    using (var streamWriter = new StreamWriter(RFPResponseAPPLogPath))
                    {
                        streamWriter.WriteLine("Log started at " + DateTime.Now + " [" + DateTime.Now.Ticks.ToString() + "]");
                    }
                }

                // Read existing content
                using (var file = new System.IO.StreamReader(RFPResponseAPPLogPath))
                {
                    RFPResponseAPPLog = file.ReadToEnd().Split('\n');

                    if (RFPResponseAPPLog[RFPResponseAPPLog.Length - 1].Trim() == "")
                    {
                        RFPResponseAPPLog = RFPResponseAPPLog.Take(RFPResponseAPPLog.Length - 1).ToArray();
                    }
                }

                // If log has more than 1000 lines, keep only the recent 1000 lines
                if (RFPResponseAPPLog.Length > 1000)
                {
                    RFPResponseAPPLog = RFPResponseAPPLog.Take(1000).ToArray();
                }

                // Append the text to csv file
                using (var streamWriter = new StreamWriter(RFPResponseAPPLogPath))
                {
                    // Remove line breaks from the log text
                    LogText = LogText.Replace("\n", " ");

                    streamWriter.WriteLine(LogText);
                    streamWriter.WriteLine(string.Join("\n", RFPResponseAPPLog));
                }
            });
        }
    }
}