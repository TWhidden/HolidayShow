using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Core.Data
{
    public partial class Devices
    {
        public Devices()
        {
            DeviceIoPorts = new HashSet<DeviceIoPorts>();
            DevicePatterns = new HashSet<DevicePatterns>();
        }

        public int DeviceId { get; set; }
        public string Name { get; set; }

        public ICollection<DeviceIoPorts> DeviceIoPorts { get; set; }
        public ICollection<DevicePatterns> DevicePatterns { get; set; }
    }
}
