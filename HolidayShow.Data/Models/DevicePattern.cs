using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Models
{
    public partial class DevicePattern
    {
        public DevicePattern()
        {
            this.DevicePatternSequences = new List<DevicePatternSequence>();
            this.SetSequences = new List<SetSequence>();
        }

        public int DevicePatternId { get; set; }
        public int DeviceId { get; set; }
        public string PatternName { get; set; }
        public virtual Device Device { get; set; }
        public virtual ICollection<DevicePatternSequence> DevicePatternSequences { get; set; }
        public virtual ICollection<SetSequence> SetSequences { get; set; }
    }
}
