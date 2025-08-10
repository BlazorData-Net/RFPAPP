using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RFPResponsePOC.Client.Models;
using RFPResponsePOC.Models;

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

        public CalculateProposale(string basePath = "/RFPResponsePOC")
        {
            _basePath = basePath;
        }

        public async Task<List<RoomAssignment>> CalculateAsync(string rfpText)
        {
            var assignments = new List<RoomAssignment>();

            if (string.IsNullOrWhiteSpace(rfpText))
                return assignments;

            if (!File.Exists($"{_basePath}//Capacity.json"))
                return assignments;

            List<RoomsRequest> requests;
            CapacityRoot capacity;

            try
            {
                requests = JsonConvert.DeserializeObject<List<RoomsRequest>>(rfpText);
                var capacityJson = await File.ReadAllTextAsync($"{_basePath}//Capacity.json");
                capacity = JsonConvert.DeserializeObject<CapacityRoot>(capacityJson);
            }
            catch
            {
                return assignments;
            }

            var schedule = new Dictionary<string, List<(DateTime start, DateTime end)>>();

            foreach (var req in requests)
            {
                var start = req.StartDate.Date.Add(req.StartTime);
                var end = req.EndDate.Date.Add(req.EndTime);
                Room selected = null;

                foreach (var room in capacity.Rooms)
                {
                    if (!MeetsCapacity(room, req))
                        continue;

                    if (IsAvailable(room, start, end, schedule, capacity.Rooms))
                    {
                        selected = room;
                        break;
                    }
                }

                if (selected != null)
                {
                    BlockRoom(selected, start, end, schedule, capacity.Rooms);
                }

                assignments.Add(new RoomAssignment
                {
                    Request = req,
                    AssignedRoom = selected?.Name
                });
            }

            return assignments;
        }

        private bool MeetsCapacity(Room room, RoomsRequest req)
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

        private bool IsAvailable(Room room, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule, List<Room> allRooms)
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

        private bool IsNameAvailable(string name, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule)
        {
            if (!schedule.TryGetValue(name, out var list))
                return true;

            return !list.Any(i => start < i.end && end > i.start);
        }

        private void BlockRoom(Room room, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule, List<Room> allRooms)
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

        private void AddEntry(string name, DateTime start, DateTime end, Dictionary<string, List<(DateTime start, DateTime end)>> schedule)
        {
            if (!schedule.TryGetValue(name, out var list))
            {
                list = new List<(DateTime, DateTime)>();
                schedule[name] = list;
            }
            list.Add((start, end));
        }
    }
}

