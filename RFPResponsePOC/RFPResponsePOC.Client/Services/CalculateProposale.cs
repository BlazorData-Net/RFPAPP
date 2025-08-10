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
    /// <summary>
    /// Represents the assignment of a room request to a specific room.
    /// Used to track which room (if any) was assigned to each request.
    /// </summary>
    public class RoomAssignment
    {
        /// <summary>
        /// The original room request containing requirements and timing
        /// </summary>
        public RoomsRequest Request { get; set; }
        
        /// <summary>
        /// The name of the assigned room, or null if no suitable room was found
        /// </summary>
        public string AssignedRoom { get; set; }
    }

    /// <summary>
    /// Service responsible for calculating room assignments based on RFP (Request for Proposal) requirements.
    /// Processes room requests and assigns available rooms that meet capacity and scheduling constraints.
    /// </summary>
    public class CalculateProposale
    {
        /// <summary>
        /// Base path for accessing configuration files (e.g., Capacity.json)
        /// </summary>
        private readonly string _basePath;
        
        /// <summary>
        /// Service for logging operations and errors during the calculation process
        /// </summary>
        private readonly LogService _logService;

        /// <summary>
        /// Initializes a new instance of the CalculateProposale service
        /// </summary>
        /// <param name="basePath">Base path for configuration files (defaults to "/RFPResponsePOC")</param>
        /// <param name="logService">Logging service instance (creates new if null)</param>
        public CalculateProposale(string basePath = "/RFPResponsePOC", LogService logService = null)
        {
            _basePath = basePath;
            _logService = logService ?? new LogService();
        }

        /// <summary>
        /// Main calculation method that processes RFP text and assigns rooms to requests.
        /// Implements a first-fit algorithm where rooms are assigned to requests in order.
        /// </summary>
        /// <param name="rfpText">JSON string containing an array of room requests</param>
        /// <returns>List of room assignments with original requests and assigned room names</returns>
        public async Task<List<RoomAssignment>> CalculateAsync(string rfpText)
        {
            // Initialize the result list to ensure we always return a valid collection
            var assignments = new List<RoomAssignment>();

            try
            {
                await _logService.WriteToLogAsync($"[{DateTime.Now}] CalculateProposale.CalculateAsync started");

                // Validate input - early return if RFP text is empty or null
                if (string.IsNullOrWhiteSpace(rfpText))
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] WARNING: Empty or null RFP text provided");
                    return assignments;
                }

                // Construct path to capacity configuration file
                var capacityFilePath = $"{_basePath}//Capacity.json";
                if (!File.Exists(capacityFilePath))
                {
                    await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Capacity.json file not found at path: {capacityFilePath}");
                    return assignments;
                }

                // Declare variables for deserialized data
                List<RoomsRequest> requests;
                CapacityRoot capacity;

                // Deserialize JSON data with comprehensive error handling
                try
                {
                    // Parse the RFP text into a list of room requests
                    requests = JsonConvert.DeserializeObject<List<RoomsRequest>>(rfpText);
                    if (requests == null)
                    {
                        await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to deserialize RFP text - result was null");
                        return assignments;
                    }

                    // Load and parse the room capacity configuration
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

                // Initialize scheduling dictionary to track room bookings
                // Key: Room name or room group name
                // Value: List of time intervals when the room is booked
                var schedule = new Dictionary<string, List<(DateTime start, DateTime end)>>();

                // Process each room request in order (first-fit algorithm)
                foreach (var req in requests)
                {
                    try
                    {
                        // Calculate actual start and end times by combining date and time components
                        var start = req.StartDate.Date.Add(req.StartTime);
                        var end = req.EndDate.Date.Add(req.EndTime);
                        Room selected = null;

                        await _logService.WriteToLogAsync($"[{DateTime.Now}] Processing request: {req.Name} for {req.Attendance} people in {req.RoomType} setup from {start} to {end}");

                        // Search through all available rooms to find the first suitable match
                        foreach (var room in capacity.Rooms)
                        {
                            try
                            {
                                // Check if room meets capacity requirements for the requested setup
                                if (!MeetsCapacity(room, req))
                                    continue;

                                // Check if room is available during the requested time period
                                if (IsAvailable(room, start, end, schedule, capacity.Rooms))
                                {
                                    selected = room;
                                    await _logService.WriteToLogAsync($"[{DateTime.Now}] Room assigned: {selected.Name} for request {req.Name}");
                                    break; // Stop searching once we find a suitable room
                                }
                            }
                            catch (Exception roomEx)
                            {
                                await _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Failed to process room {room?.Name ?? "unknown"} for request {req.Name} - {roomEx.Message}");
                            }
                        }

                        // If a room was selected, block it in the schedule to prevent double-booking
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

                        // Create assignment record regardless of whether a room was found
                        assignments.Add(new RoomAssignment
                        {
                            Request = req,
                            AssignedRoom = selected?.Name // Will be null if no room was assigned
                        });
                    }
                    catch (Exception reqEx)
                    {
                        // Handle errors processing individual requests gracefully
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
                // Catch-all error handler to ensure method never throws unhandled exceptions
                await _logService.WriteToLogAsync($"[{DateTime.Now}] CRITICAL ERROR: Unexpected error in CalculateAsync - {ex.Message}");
                return assignments;
            }
        }

        /// <summary>
        /// Determines if a room meets the capacity requirements for a specific request.
        /// Uses reflection to dynamically access the capacity property based on room type.
        /// </summary>
        /// <param name="room">The room to check capacity for</param>
        /// <param name="req">The room request with attendance and setup requirements</param>
        /// <returns>True if the room can accommodate the request, false otherwise</returns>
        private bool MeetsCapacity(Room room, RoomsRequest req)
        {
            try
            {
                // Validate input parameters
                if (room?.Capacities == null || string.IsNullOrEmpty(req.RoomType))
                    return false;

                // Use reflection to find the property that matches the requested room type
                // This allows for flexible room types without hardcoding specific setup names
                var prop = typeof(Capacities).GetProperty(req.RoomType, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                    return false;

                // Get the capacity value for the specific setup type
                var value = prop.GetValue(room.Capacities);
                if (value == null)
                    return false;

                // Convert to integer and compare with requested attendance
                var cap = Convert.ToInt32(value);
                return cap >= req.Attendance;
            }
            catch (Exception ex)
            {
                // Log errors but don't throw - return false to indicate room doesn't meet capacity
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in MeetsCapacity for room {room?.Name} and request {req?.Name} - {ex.Message}").ConfigureAwait(false);
                return false;
            }
        }

        /// <summary>
        /// Checks if a room is available during the specified time period.
        /// Considers room groups and child rooms to prevent conflicts in connected spaces.
        /// </summary>
        /// <param name="room">The room to check availability for</param>
        /// <param name="start">Start time of the requested booking</param>
        /// <param name="end">End time of the requested booking</param>
        /// <param name="schedule">Current booking schedule</param>
        /// <param name="allRooms">Complete list of rooms for group relationship checking</param>
        /// <returns>True if the room is available, false if there are conflicts</returns>
        private bool IsAvailable(Room room, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule, List<Room> allRooms)
        {
            try
            {
                // Check if the room itself is available
                if (!IsNameAvailable(room.Name, start, end, schedule))
                    return false;

                // Check if the room group (parent space) is available
                // If a room is part of a group, booking the group blocks all individual rooms
                if (!string.IsNullOrEmpty(room.RoomGroup) && !IsNameAvailable(room.RoomGroup, start, end, schedule))
                    return false;

                // Check if any child rooms are available
                // If this room acts as a group, all child rooms must be available
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
                // Log errors but return false to be safe
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in IsAvailable for room {room?.Name} - {ex.Message}").ConfigureAwait(false);
                return false;
            }
        }

        /// <summary>
        /// Checks if a specific room name is available during the given time period.
        /// Implements interval overlap detection to identify scheduling conflicts.
        /// </summary>
        /// <param name="name">Room name to check</param>
        /// <param name="start">Start time of the requested booking</param>
        /// <param name="end">End time of the requested booking</param>
        /// <param name="schedule">Current booking schedule</param>
        /// <returns>True if no time conflicts exist, false if there are overlapping bookings</returns>
        private bool IsNameAvailable(string name, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule)
        {
            try
            {
                // If no bookings exist for this name, it's available
                if (!schedule.TryGetValue(name, out var list))
                    return true;

                // Check for interval overlap using the standard algorithm:
                // Two intervals overlap if: start1 < end2 AND end1 > start2
                // We want to return false if ANY existing booking overlaps with our requested time
                return !list.Any(i => start < i.end && end > i.start);
            }
            catch (Exception ex)
            {
                // Log errors but return false to be safe
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in IsNameAvailable for {name} - {ex.Message}").ConfigureAwait(false);
                return false;
            }
        }

        /// <summary>
        /// Blocks a room and all related spaces during the specified time period.
        /// Handles room groups and child rooms to prevent double-booking of connected spaces.
        /// </summary>
        /// <param name="room">The room to block</param>
        /// <param name="start">Start time of the booking</param>
        /// <param name="end">End time of the booking</param>
        /// <param name="schedule">Schedule to update with the new booking</param>
        /// <param name="allRooms">Complete list of rooms for group relationship processing</param>
        private void BlockRoom(Room room, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule, List<Room> allRooms)
        {
            try
            {
                // Block the specific room
                AddEntry(room.Name, start, end, schedule);

                // Block the room group if this room belongs to one
                // This prevents other rooms in the group from being booked
                if (!string.IsNullOrEmpty(room.RoomGroup))
                {
                    AddEntry(room.RoomGroup, start, end, schedule);
                }

                // Block all child rooms if this room acts as a group
                // This prevents individual rooms from being booked when the group is reserved
                var children = allRooms.Where(r => r.RoomGroup == room.Name).Select(r => r.Name);
                foreach (var child in children)
                {
                    AddEntry(child, start, end, schedule);
                }
            }
            catch (Exception ex)
            {
                // Log errors but don't throw - room blocking failure shouldn't crash the entire process
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in BlockRoom for {room?.Name} - {ex.Message}").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a booking entry to the schedule for a specific room or room group.
        /// Creates a new entry list if this is the first booking for the given name.
        /// </summary>
        /// <param name="name">Room or room group name</param>
        /// <param name="start">Start time of the booking</param>
        /// <param name="end">End time of the booking</param>
        /// <param name="schedule">Schedule dictionary to update</param>
        private void AddEntry(string name, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule)
        {
            try
            {
                // Get existing booking list or create a new one
                if (!schedule.TryGetValue(name, out var list))
                {
                    list = new List<(DateTime, DateTime)>();
                    schedule[name] = list;
                }
                
                // Add the new booking to the list
                list.Add((start, end));
            }
            catch (Exception ex)
            {
                // Log errors but don't throw - individual entry failure shouldn't crash the process
                _logService.WriteToLogAsync($"[{DateTime.Now}] ERROR: Exception in AddEntry for {name} - {ex.Message}").ConfigureAwait(false);
            }
        }
    }
}

