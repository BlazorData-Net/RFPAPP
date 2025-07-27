using Newtonsoft.Json;
using System.Collections.Generic;

namespace RFPResponsePOC.Models
{
    public class CapacityRoot
    {
        [JsonProperty("rooms")] public List<Room> Rooms { get; set; }
    }

    public class Room
    {
        [JsonProperty("name")] public string Name { get; set; }
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
}
