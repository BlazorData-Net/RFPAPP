using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RFPResponseAPP.Models;

namespace RFPResponseAPP.Client.Models
{
    public static class RoomGroupingHelper
    {
        public static CapacityRoot GroupRooms(IEnumerable<Room> rooms)
        {
            if (rooms == null)
                return new CapacityRoot { Rooms = new List<Room>() };

            // Words to exclude from grouping
            var excluded = new HashSet<string> { "San", "Del", "Santa" };

            // Group by the first word, unless it's in the excluded list; if so, use the second word
            var groupedByName = rooms
                .GroupBy(r =>
                {
                    var words = (r.Name ?? string.Empty).Split(' ', 3, System.StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length == 0)
                        return string.Empty;
                    if (excluded.Contains(words[0]) && words.Length > 1)
                        return words[1];
                    return words[0];
                })
                .ToList();

            // For each group, set RoomGroup to the Name of the room with the highest SquareFeet
            foreach (var group in groupedByName)
            {
                var maxSqFtRoom = group
                    .OrderByDescending(r => r.SquareFeet ?? 0)
                    .FirstOrDefault();

                if (maxSqFtRoom != null)
                {
                    string targetGroup = maxSqFtRoom.Name;

                    foreach (var room in group)
                    {
                        if (room.Name != maxSqFtRoom.Name)
                        {
                            // If the room name contains "Foyer", do not change its RoomGroup
                            if (!room.Name.ToLower().Contains("foyer"))
                            {
                                room.RoomGroup = targetGroup;
                            }
                        }
                    }
                }
            }

            // Return a CapacityRoot with the updated rooms
            return new CapacityRoot { Rooms = rooms.ToList() };
        }
    }
}