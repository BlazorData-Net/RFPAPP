using Newtonsoft.Json;

namespace RFPResponseAPP.Client.Models
{
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
