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
        [JsonProperty("squareFeet")] public int SquareFeet { get; set; }
        [JsonProperty("capacities")] public Capacities Capacities { get; set; }
    }

    public class Capacities
    {
        [JsonProperty("banquet")] public int Banquet { get; set; }
        [JsonProperty("conference")] public int Conference { get; set; }
        [JsonProperty("square")] public int Square { get; set; }
        [JsonProperty("reception")] public int Reception { get; set; }
        [JsonProperty("schoolRoom")] public int SchoolRoom { get; set; }
        [JsonProperty("theatre")] public int Theatre { get; set; }
        [JsonProperty("uShape")] public int UShape { get; set; }
    }
}
