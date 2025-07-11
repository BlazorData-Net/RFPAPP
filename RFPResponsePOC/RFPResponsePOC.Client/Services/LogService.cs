using Newtonsoft.Json;
using OpenAI.Files;

namespace RFPResponsePOC.Model
{
    public class LogService
    {
        // Properties
        public string[] RFPResponsePOCLog { get; set; }

        public async Task WriteToLogAsync(string LogText)
        {
            await Task.Run(() =>
            {
                // Open the file to get existing content
                var RFPResponsePOCLogPath =
                    $"RFPResponsePOC/RFPResponsePOCLog.csv";

                using (var file = new System.IO.StreamReader(RFPResponsePOCLogPath))
                {
                    RFPResponsePOCLog = file.ReadToEnd().Split('\n');

                    if (RFPResponsePOCLog[RFPResponsePOCLog.Length - 1].Trim() == "")
                    {
                        RFPResponsePOCLog = RFPResponsePOCLog.Take(RFPResponsePOCLog.Length - 1).ToArray();
                    }
                }

                // If log has more than 1000 lines, keep only the recent 1000 lines
                if (RFPResponsePOCLog.Length > 1000)
                {
                    RFPResponsePOCLog = RFPResponsePOCLog.Take(1000).ToArray();
                }

                // Append the text to csv file
                using (var streamWriter = new StreamWriter(RFPResponsePOCLogPath))
                {
                    // Remove line breaks from the log text
                    LogText = LogText.Replace("\n", " ");

                    streamWriter.WriteLine(LogText);
                    streamWriter.WriteLine(string.Join("\n", RFPResponsePOCLog));
                }
            });
        }
    }
}