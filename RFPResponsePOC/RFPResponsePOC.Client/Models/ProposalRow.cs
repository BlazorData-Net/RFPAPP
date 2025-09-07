using System;

namespace RFPResponseAPP.Client.Models
{
    public class ProposalRow
    {
        public string Name { get; set; }
        public string SelectedRoom { get; set; }
        public string ManualRoom { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }
        public string RoomType { get; set; }
        public int Attendance { get; set; }
        public string Notes { get; set; }
        public DateTime StartTimeBinding { get => DateTime.Today.Add(StartTime); set => StartTime = value.TimeOfDay; }
        public DateTime EndTimeBinding { get => DateTime.Today.Add(EndTime); set => EndTime = value.TimeOfDay; }
    }
}
