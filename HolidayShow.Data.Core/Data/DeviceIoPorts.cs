using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Core.Data
{
    public partial class DeviceIoPorts
    {
        public DeviceIoPorts()
        {
            DevicePatternSequences = new HashSet<DevicePatternSequences>();
        }

        public int DeviceIoPortId { get; set; }
        public int DeviceId { get; set; }
        public int CommandPin { get; set; }
        public string Description { get; set; }
        public bool IsNotVisable { get; set; }
        public bool IsDanger { get; set; }

        public Devices Device { get; set; }
        public ICollection<DevicePatternSequences> DevicePatternSequences { get; set; }
    }
}
