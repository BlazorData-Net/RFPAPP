using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RFPResponsePOC.Models;

namespace RFPResponsePOC.Client.Models
{
    public static class RoomGroupingHelper
    {
        public static CapacityRoot GroupRooms(IEnumerable<Room> rooms)
        {
            if (rooms == null)
                return new CapacityRoot { Rooms = new List<Room>() };

            // Group by the beginning of the room name (first word)
            var groupedByName = rooms
                .GroupBy(r => (r.Name ?? string.Empty).Split(' ').FirstOrDefault() ?? string.Empty)
                .ToList();

            // For each group, set RoomGroup to the RoomGroup of the room with the highest SquareFeet
            foreach (var group in groupedByName)
            {
                var maxSqFtRoom = group
                    .OrderByDescending(r => r.SquareFeet ?? 0)
                    .FirstOrDefault();

                if (maxSqFtRoom != null)
                {
                    // Use the name of the room with the highest SquareFeet as the target group
                    string targetGroup = maxSqFtRoom.Name;

                    foreach (var room in group)
                    {
                        // Do not assign if this is the max room itself
                        if (room.Name != maxSqFtRoom.Name)
                        {
                            // Assign the RoomGroup to the target group
                            room.RoomGroup = targetGroup;
                        }
                    }
                }
            }

            // Return a CapacityRoot with the updated rooms
            return new CapacityRoot { Rooms = rooms.ToList() };
        }
    }
}