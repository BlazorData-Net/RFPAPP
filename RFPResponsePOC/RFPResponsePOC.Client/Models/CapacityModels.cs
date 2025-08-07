using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Add this for validation attributes

namespace RFPResponsePOC.Models
{
    public class CapacityRoot
    {
        [JsonProperty("rooms")] public List<Room> Rooms { get; set; }
    }

    public class Room
    {
        [Required(ErrorMessage = "Room name is required")] // Make Name required
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("roomGroup")] public string RoomGroup { get; set; }
        [JsonProperty("squareFeet")] public double? SquareFeet { get; set; }
        [JsonProperty("length")] public double? Length { get; set; }
        [JsonProperty("width")] public double? Width { get; set; }
        [JsonProperty("ceilingHeight")] public double? CeilingHeight { get; set; }
        [JsonProperty("floorLevel")] public string FloorLevel { get; set; }
        [JsonProperty("hasNaturalLight")] public bool? HasNaturalLight { get; set; }
        [JsonProperty("hasPillars")] public bool? HasPillars { get; set; }
        [JsonProperty("capacities")] public Capacities Capacities { get; set; }
    }

    public class Capacities
    {
        [JsonProperty("banquet")] public int? Banquet { get; set; }
        [JsonProperty("conference")] public int? Conference { get; set; }
        [JsonProperty("square")] public int? Square { get; set; }
        [JsonProperty("reception")] public int? Reception { get; set; }
        [JsonProperty("schoolRoom")] public int? SchoolRoom { get; set; }
        [JsonProperty("theatre")] public int? Theatre { get; set; }
        [JsonProperty("uShape")] public int? UShape { get; set; }
        [JsonProperty("hollowSquare")] public int? HollowSquare { get; set; }
        [JsonProperty("boardroom")] public int? Boardroom { get; set; }
        [JsonProperty("crescentRounds")] public int? CrescentRounds { get; set; }
    }

    public class RoomsRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [JsonProperty("start_time")]
        public TimeSpan StartTime { get; set; }

        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; }

        [JsonProperty("end_time")]
        public TimeSpan EndTime { get; set; }

        [JsonProperty("room_type")]
        public string RoomType { get; set; }

        [JsonProperty("attendance")]
        public int Attendance { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }

    public class RoomsResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [JsonProperty("start_time")]
        public TimeSpan StartTime { get; set; }

        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; }

        [JsonProperty("end_time")]
        public TimeSpan EndTime { get; set; }

        [JsonProperty("room_type")]
        public string RoomType { get; set; }

        [JsonProperty("attendance")]
        public int Attendance { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }
}
