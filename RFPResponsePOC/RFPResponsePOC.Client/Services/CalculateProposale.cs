using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RFPResponsePOC.Client.Models;
using RFPResponsePOC.Models;
using RFPResponsePOC.Model;

namespace RFPResponsePOC.Client.Services
{
    public class RoomAssignment
    {
        public RoomsRequest Request { get; set; }
        public string AssignedRoom { get; set; }
    }

    public class CalculateProposale
    {
        private readonly string _basePath;
        private readonly LogService _logService;

        public CalculateProposale(string basePath = "/RFPResponsePOC", LogService logService = null)
        {
            _basePath = basePath;
            _logService = logService ?? new LogService();
        }

        public async Task<List<RoomAssignment>> CalculateAsync(string rfpText)
        {
            var assignments = new List<RoomAssignment>();

            try
            {
                await _logService.WriteToLogAsync($"[{DateTime.Now}] CalculateProposale.CalculateAsync started");

                if (string.IsNullOrWhiteSpace(rfpText))
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] WARNING: Empty or null RFP text provided");
                    return assignments;
                }

                var capacityFilePath = $"{_basePath}//Capacity.json";
                if (!File.Exists(capacityFilePath))
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Capacity.json file not found at path: {capacityFilePath}");
                    return assignments;
                }

                List<RoomsRequest> requests;
                CapacityRoot capacity;

                try
                {
                    requests = JsonConvert.DeserializeObject<List<RoomsRequest>>(rfpText);
                    if (requests == null)
                    {
                        await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to deserialize RFP text - result was null");
                        return assignments;
                    }

                    var capacityJson = await File.ReadAllTextAsync(capacityFilePath);
                    capacity = JsonConvert.DeserializeObject<CapacityRoot>(capacityJson);
                    if (capacity == null)
                    {
                        await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to deserialize Capacity.json - result was null");
                        return assignments;
                    }

                    await _logService.WriteToLogAsync($"[{DateTime.Now}] Successfully loaded {requests.Count} room requests and {capacity.Rooms?.Count ?? 0} rooms");
                }
                catch (JsonException jsonEx)
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: JSON deserialization failed - {jsonEx.Message}");
                    return assignments;
                }
                catch (FileNotFoundException fileEx)
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: File not found - {fileEx.Message}");
                    return assignments;
                }
                catch (UnauthorizedAccessException accessEx)
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: File access denied - {accessEx.Message}");
                    return assignments;
                }
                catch (Exception ex)
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Unexpected error during file operations - {ex.Message}");
                    return assignments;
                }

                var schedule = new Dictionary<string, List<(DateTime start, DateTime end)>>();

                foreach (var req in requests)
                {
                    try
                    {
                        var start = req.StartDate.Date.Add(req.StartTime);
                        var end = req.EndDate.Date.Add(req.EndTime);
                        Room selected = null;

                        await _logService.WriteToLogAsync($"[{DateTime.Now}] Processing request: {req.Name} for {req.Attendance} people in {req.RoomType} setup from {start} to {end}");

                        foreach (var room in capacity.Rooms)
                        {
                            try
                            {
                                if (!MeetsCapacity(room, req))
                                    continue;

                                if (IsAvailable(room, start, end, schedule, capacity.Rooms))
                                {
                                    selected = room;
                                    await _logService.WriteToLogAsync($"[{DateTime.Now}] Room assigned: {selected.Name} for request {req.Name}");
                                    break;
                                }
                            }
                            catch (Exception roomEx)
                            {
                                await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to process room {room?.Name ?? "unknown"} for request {req.Name} - {roomEx.Message}");
                            }
                        }

                        if (selected != null)
                        {
                            try
                            {
                                BlockRoom(selected, start, end, schedule, capacity.Rooms);
                            }
                            catch (Exception blockEx)
                            {
                                await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to block room {selected.Name} - {blockEx.Message}");
                            }
                        }
                        else
                        {
                            await _logService.WriteToLogAsync($"[{DateTime.Now}] WARNING: No suitable room found for request {req.Name}");
                        }

                        assignments.Add(new RoomAssignment
                        {
                            Request = req,
                            AssignedRoom = selected?.Name
                        });
                    }
                    catch (Exception reqEx)
                    {
                        await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to process request {req?.Name ?? "unknown"} - {reqEx.Message}");
                        assignments.Add(new RoomAssignment
                        {
                            Request = req,
                            AssignedRoom = null
                        });
                    }
                }

                await _logService.WriteToLogAsync($"[{DateTime.Now}] CalculateProposale.CalculateAsync completed successfully with {assignments.Count} assignments");
                return assignments;
            }
            catch (Exception ex)
            {
                await _logService.WriteToLogAsync($"[{DateTime.Now}] CRITICAL ERROR: Unexpected error in CalculateAsync - {ex.Message}");
                return assignments;
            }
        }

        private bool MeetsCapacity(Room room, RoomsRequest req)
        {
            try
            {
                if (room?.Capacities == null || string.IsNullOrEmpty(req.RoomType))
                    return false;

                var prop = typeof(Capacities).GetProperty(req.RoomType, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                    return false;

                var value = prop.GetValue(room.Capacities);
                if (value == null)
                    return false;

                var cap = Convert.ToInt32(value);
                return cap >= req.Attendance;
            }
            catch (Exception ex)
            {
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in MeetsCapacity for room {room?.Name} and request {req?.Name} - {ex.Message}").ConfigureAwait(false);
                return false;
            }
        }

        private bool IsAvailable(Room room, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule, List<Room> allRooms)
        {
            try
            {
                if (!IsNameAvailable(room.Name, start, end, schedule))
                    return false;

                if (!string.IsNullOrEmpty(room.RoomGroup) && !IsNameAvailable(room.RoomGroup, start, end, schedule))
                    return false;

                var children = allRooms.Where(r => r.RoomGroup == room.Name).Select(r => r.Name);
                foreach (var child in children)
                {
                    if (!IsNameAvailable(child, start, end, schedule))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in IsAvailable for room {room?.Name} - {ex.Message}").ConfigureAwait(false);
                return false;
            }
        }

        private bool IsNameAvailable(string name, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule)
        {
            try
            {
                if (!schedule.TryGetValue(name, out var list))
                    return true;

                return !list.Any(i => start < i.end && end > i.start);
            }
            catch (Exception ex)
            {
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in IsNameAvailable for {name} - {ex.Message}").ConfigureAwait(false);
                return false;
            }
        }

        private void BlockRoom(Room room, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule, List<Room> allRooms)
        {
            try
            {
                AddEntry(room.Name, start, end, schedule);

                if (!string.IsNullOrEmpty(room.RoomGroup))
                {
                    AddEntry(room.RoomGroup, start, end, schedule);
                }

                var children = allRooms.Where(r => r.RoomGroup == room.Name).Select(r => r.Name);
                foreach (var child in children)
                {
                    AddEntry(child, start, end, schedule);
                }
            }
            catch (Exception ex)
            {
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in BlockRoom for {room?.Name} - {ex.Message}").ConfigureAwait(false);
            }
        }

        private void AddEntry(string name, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule)
        {
            try
            {
                if (!schedule.TryGetValue(name, out var list))
                {
                    list = new List<(DateTime, DateTime)>();
                    schedule[name] = list;
                }
                list.Add((start, end));
            }
            catch (Exception ex)
            {
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in AddEntry for {name} - {ex.Message}").ConfigureAwait(false);
            }
        }
    }
}

