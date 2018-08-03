using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Models
{
    public partial class DeviceIoPort
    {
        public DeviceIoPort()
        {
            this.DevicePatternSequences = new List<DevicePatternSequence>();
        }

        public int DeviceIoPortId { get; set; }
        public int DeviceId { get; set; }
        public int CommandPin { get; set; }
        public string Description { get; set; }
        public bool IsNotVisable { get; set; }
        public bool IsDanger { get; set; }
        public virtual Device Device { get; set; }
        public virtual ICollection<DevicePatternSequence> DevicePatternSequences { get; set; }
    }
}
