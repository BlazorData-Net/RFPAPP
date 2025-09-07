using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Radzen;
using RFPResponseAPP.AI;
using RFPResponseAPP.Client.Models;
using RFPResponseAPP.Model;
using RFPResponseAPP.Models;

namespace RFPResponseAPP.Client.Services
{
    public static class Calculations
    {
        public static async Task<(AIResponse result, string processedText, List<ProposalRow> proposalRows, List<string> roomOptions)> DetectRoomRequests(
            string rfpText,
            HttpClient http,
            SettingsService settingsService,
            LogService logService,
            NotificationService notificationService,
            Func<Task> saveProposalRowsAsync,
            Action sortProposalRows)
        {
            AIResponse result = new AIResponse();
            string processedText = string.Empty;
            List<ProposalRow> proposalRows = new();
            List<string> roomOptions = new();

            try
            {
                await logService.WriteToLogAsync($"[{DateTime.Now}] ProcessOCRText method started");

                var objOrchestratorMethods = new OrchestratorMethods(settingsService, logService);
                var localSettings = new SettingsService();

                var proposalPrompt = await http.GetStringAsync("Prompts/Proposal.prompt");
                proposalPrompt = proposalPrompt.Replace("{{OCRResult}}", rfpText);

                result = await objOrchestratorMethods.CallOpenAIAsync(localSettings, proposalPrompt);

                processedText = $"[{result.Response}]";

                if (result.Error == "")
                {
                    var calcResult = await CalculateProposal(processedText, rfpText, logService, notificationService, saveProposalRowsAsync, sortProposalRows);
                    proposalRows = calcResult.ProposalRows;
                    roomOptions = calcResult.RoomOptions;
                }
                else
                {
                    await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: ProcessOCRText failed - {result.Error}");
                    notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = $"Failed to process... {result.Error}",
                        Duration = 4000
                    });
                }
            }
            catch (Exception ex)
            {
                await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in ProcessOCRText - {ex.Message}");
                notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 8000
                });
            }

            return (result, processedText, proposalRows, roomOptions);
        }

        public static async Task<List<string>> ReCalculateProposal(List<ProposalRow> proposalRows, LogService logService, NotificationService notificationService)
        {
            try
            {
                return await CalculateRoomsForExistingRows(proposalRows, logService, notificationService);
            }
            catch (Exception ex)
            {
                await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in ReCalculateProposal - {ex.Message}");
                await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Stack trace - {ex.StackTrace}");
                notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Failed to calculate rooms: {ex.Message}",
                    Duration = 8000
                });
                return new List<string>();
            }
        }

        public static async Task<(List<ProposalRow> ProposalRows, List<string> RoomOptions)> CalculateProposal(
            string processedText,
            string rfpText,
            LogService logService,
            NotificationService notificationService,
            Func<Task> saveProposalRowsAsync,
            Action sortProposalRows)
        {
            try
            {
                await logService.WriteToLogAsync($"[{DateTime.Now}] CalculateProposal method started");
                await logService.WriteToLogAsync($"[{DateTime.Now}] RFPText content length: {rfpText?.Length ?? 0}");
                await logService.WriteToLogAsync($"[{DateTime.Now}] RFPText content (first 1000 chars): {rfpText?.Substring(0, Math.Min(1000, rfpText?.Length ?? 0))}");

                string processedJsonText = processedText;

                try
                {
                    var testParse = JsonConvert.DeserializeObject(processedText);
                    await logService.WriteToLogAsync($"[{DateTime.Now}] JSON parsing test successful. Object type: {testParse?.GetType().Name}");
                    if (testParse is Newtonsoft.Json.Linq.JObject jObject)
                    {
                        await logService.WriteToLogAsync($"[{DateTime.Now}] Detected JSON object. Properties: {string.Join(", ", jObject.Properties().Select(p => p.Name))}");
                        var arrayProperty = jObject.Properties().FirstOrDefault(p => p.Value is Newtonsoft.Json.Linq.JArray);
                        if (arrayProperty != null)
                            processedJsonText = arrayProperty.Value.ToString();
                        else
                            processedJsonText = $"[{processedText}]";
                    }
                }
                catch (JsonReaderException jsonReadEx)
                {
                    await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: JSON format is invalid - {jsonReadEx.Message}");
                    notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "JSON Format Error",
                        Detail = "The text does not contain valid JSON.",
                        Duration = 8000
                    });
                    return (new List<ProposalRow>(), new List<string>());
                }

                await logService.WriteToLogAsync($"[{DateTime.Now}] Processed JSON text (first 1000 chars): {processedJsonText?.Substring(0, Math.Min(1000, processedJsonText?.Length ?? 0))}");

                var calculator = new CalculateProposal("/RFPResponseAPP", logService);

                List<RoomAssignment> assigned;
                try
                {
                    assigned = await calculator.CalculateAsync(processedJsonText);
                    await logService.WriteToLogAsync($"[{DateTime.Now}] Calculator.CalculateAsync completed successfully with {assigned?.Count ?? 0} results");
                }
                catch (FormatException formatEx)
                {
                    await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Format exception in CalculateAsync - {formatEx.Message}");
                    await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Stack trace - {formatEx.StackTrace}");
                    notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Data Format Error",
                        Detail = $"Invalid data format detected: {formatEx.Message}. Please check the date/time or numeric values in your proposal.",
                        Duration = 8000
                    });
                    return (new List<ProposalRow>(), new List<string>());
                }
                catch (Exception calcEx)
                {
                    await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in CalculateAsync - {calcEx.Message}");
                    await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Stack trace - {calcEx.StackTrace}");
                    throw;
                }

                List<string> roomOptions;
                try
                {
                    var capacityJson = await File.ReadAllTextAsync("/RFPResponseAPP/Capacity.json");
                    var capacity = JsonConvert.DeserializeObject<CapacityRoot>(capacityJson);
                    roomOptions = capacity?.Rooms?.Select(r => r.Name).OrderBy(n => n).ToList() ?? new List<string>();
                }
                catch (Exception ex)
                {
                    await logService.WriteToLogAsync($"[{DateTime.Now}] WARNING: Unable to load Capacity.json - {ex.Message}");
                    roomOptions = new List<string>();
                }

                List<ProposalRow> proposalRows;
                try
                {
                    proposalRows = assigned.Select(a => new ProposalRow
                    {
                        Name = a.Request?.Name ?? "Unknown",
                        SelectedRoom = a.AssignedRoom ?? "",
                        ManualRoom = string.Empty,
                        StartDate = a.Request?.StartDate ?? DateTime.Today,
                        StartTime = a.Request?.StartTime ?? TimeSpan.Zero,
                        EndDate = a.Request?.EndDate ?? DateTime.Today,
                        EndTime = a.Request?.EndTime ?? TimeSpan.Zero,
                        RoomType = a.Request?.RoomType ?? "",
                        Attendance = a.Request?.Attendance ?? 0,
                        Notes = a.Request?.Notes ?? ""
                    }).ToList();
                    sortProposalRows?.Invoke();
                    if (saveProposalRowsAsync != null)
                    {
                        await saveProposalRowsAsync();
                    }
                }
                catch (Exception rowEx)
                {
                    await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception creating ProposalRow objects - {rowEx.Message}");
                    await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Stack trace - {rowEx.StackTrace}");
                    throw;
                }

                await logService.WriteToLogAsync($"[{DateTime.Now}] CalculateProposal completed successfully with {assigned.Count} assignments");

                notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = "Calculation completed.",
                    Duration = 4000
                });

                return (proposalRows, roomOptions);
            }
            catch (JsonException jsonEx)
            {
                await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: JSON deserialization error in CalculateProposal - {jsonEx.Message}");
                await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: JSON error stack trace - {jsonEx.StackTrace}");
                notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "JSON Error",
                    Detail = $"Failed to parse JSON data: {jsonEx.Message}. Please ensure the proposal has been processed correctly.",
                    Duration = 8000
                });
                return (new List<ProposalRow>(), new List<string>());
            }
            catch (Exception ex)
            {
                await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in CalculateProposal - {ex.Message}");
                await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception type: {ex.GetType().Name}");
                await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Stack trace - {ex.StackTrace}");
                notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = ex.Message,
                    Duration = 8000
                });
                return (new List<ProposalRow>(), new List<string>());
            }
        }

        public static async Task<List<string>> CalculateRoomsForExistingRows(List<ProposalRow> proposalRows, LogService logService, NotificationService notificationService)
        {
            try
            {
                await logService.WriteToLogAsync($"[{DateTime.Now}] CalculateRoomsForExistingRows started with {proposalRows?.Count ?? 0} rows");

                if (proposalRows == null || !proposalRows.Any())
                {
                    await logService.WriteToLogAsync($"[{DateTime.Now}] No existing proposal rows found");
                    notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "Warning",
                        Detail = "No proposal rows found. Please add some rows or upload a proposal first.",
                        Duration = 4000
                    });
                    return new List<string>();
                }

                // Find rows that need room assignments (no manual room and no selected room)
                var rowsNeedingRooms = proposalRows.Where(row => string.IsNullOrWhiteSpace(row.ManualRoom) && string.IsNullOrWhiteSpace(row.SelectedRoom)).ToList();

                // Find rows that already have room assignments (either manual or selected)
                var rowsWithAssignments = proposalRows.Where(row => !string.IsNullOrWhiteSpace(row.ManualRoom) || !string.IsNullOrWhiteSpace(row.SelectedRoom)).ToList();

                await logService.WriteToLogAsync($"[{DateTime.Now}] Found {rowsNeedingRooms.Count} rows needing room assignments and {rowsWithAssignments.Count} rows with existing assignments");

                if (!rowsNeedingRooms.Any())
                {
                    await logService.WriteToLogAsync($"[{DateTime.Now}] No rows require room assignments");
                    notificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Info,
                        Summary = "Info",
                        Detail = "All rows already have room assignments or manual room overrides.",
                        Duration = 4000
                    });
                    
                    // Still return room options even if no calculations are needed
                    try
                    {
                        var capacityJson = await File.ReadAllTextAsync("/RFPResponseAPP/Capacity.json");
                        var capacity = JsonConvert.DeserializeObject<CapacityRoot>(capacityJson);
                        return capacity?.Rooms?.Select(r => r.Name).OrderBy(n => n).ToList() ?? new List<string>();
                    }
                    catch (Exception ex)
                    {
                        await logService.WriteToLogAsync($"[{DateTime.Now}] WARNING: Unable to load Capacity.json - {ex.Message}");
                        return new List<string>();
                    }
                }

                // Convert rows needing rooms to RoomRequest objects
                var requests = rowsNeedingRooms.Select(row => new RoomsRequest
                {
                    Name = row.Name ?? "Unknown",
                    StartDate = row.StartDate,
                    StartTime = row.StartTime,
                    EndDate = row.EndDate,
                    EndTime = row.EndTime,
                    RoomType = row.RoomType ?? "",
                    Attendance = row.Attendance,
                    Notes = row.Notes ?? ""
                }).ToList();

                // Convert rows with existing assignments to RoomAssignment objects
                var existingAssignments = rowsWithAssignments.Select(row => new RoomAssignment
                {
                    Request = new RoomsRequest
                    {
                        Name = row.Name ?? "Unknown",
                        StartDate = row.StartDate,
                        StartTime = row.StartTime,
                        EndDate = row.EndDate,
                        EndTime = row.EndTime,
                        RoomType = row.RoomType ?? "",
                        Attendance = row.Attendance,
                        Notes = row.Notes ?? ""
                    },
                    AssignedRoom = !string.IsNullOrWhiteSpace(row.ManualRoom) ? row.ManualRoom : row.SelectedRoom
                }).ToList();

                var requestsJson = JsonConvert.SerializeObject(requests);
                await logService.WriteToLogAsync($"[{DateTime.Now}] Created requests JSON for calculation: {requestsJson.Substring(0, Math.Min(500, requestsJson.Length))}");
                await logService.WriteToLogAsync($"[{DateTime.Now}] Existing assignments to consider: {string.Join(", ", existingAssignments.Select(ea => $"{ea.Request?.Name}:{ea.AssignedRoom}"))}");

                var calculator = new CalculateProposal("/RFPResponseAPP", logService);
                
                // Use the new overloaded method that considers existing assignments
                var assignments = await calculator.CalculateAsync(requestsJson, existingAssignments);
                await logService.WriteToLogAsync($"[{DateTime.Now}] Calculator returned {assignments?.Count ?? 0} assignments");

                var assignmentDict = assignments.ToDictionary(a => a.Request?.Name ?? "", a => a.AssignedRoom);

                foreach (var row in rowsNeedingRooms)
                {
                    if (assignmentDict.TryGetValue(row.Name ?? "", out var assignedRoom) && !string.IsNullOrWhiteSpace(assignedRoom))
                    {
                        row.SelectedRoom = assignedRoom;
                        await logService.WriteToLogAsync($"[{DateTime.Now}] Assigned room '{assignedRoom}' to row '{row.Name}'");
                    }
                    else
                    {
                        await logService.WriteToLogAsync($"[{DateTime.Now}] No room assigned for row '{row.Name}'");
                    }
                }

                List<string> roomOptions;
                try
                {
                    var capacityJson = await File.ReadAllTextAsync("/RFPResponseAPP/Capacity.json");
                    var capacity = JsonConvert.DeserializeObject<CapacityRoot>(capacityJson);
                    roomOptions = capacity?.Rooms?.Select(r => r.Name).OrderBy(n => n).ToList() ?? new List<string>();
                }
                catch (Exception ex)
                {
                    await logService.WriteToLogAsync($"[{DateTime.Now}] WARNING: Unable to load Capacity.json - {ex.Message}");
                    roomOptions = new List<string>();
                }

                int assignedCount = rowsNeedingRooms.Count(r => !string.IsNullOrWhiteSpace(r.SelectedRoom));
                await logService.WriteToLogAsync($"[{DateTime.Now}] CalculateRoomsForExistingRows completed - assigned {assignedCount} of {rowsNeedingRooms.Count} rooms");

                return roomOptions;
            }
            catch (Exception ex)
            {
                await logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in CalculateRoomsForExistingRows - {ex.Message}");
                return new List<string>();
            }
        }
    }
}
