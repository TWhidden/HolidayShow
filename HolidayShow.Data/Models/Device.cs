using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Models
{
    public partial class Device
    {
        public Device()
        {
            this.DeviceIoPorts = new List<DeviceIoPort>();
            this.DevicePatterns = new List<DevicePattern>();
        }

        public int DeviceId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<DeviceIoPort> DeviceIoPorts { get; set; }
        public virtual ICollection<DevicePattern> DevicePatterns { get; set; }
    }
}
