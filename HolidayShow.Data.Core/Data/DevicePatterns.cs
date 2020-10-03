using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Core.Data
{
    public partial class DevicePatterns
    {
        public DevicePatterns()
        {
            DevicePatternSequences = new HashSet<DevicePatternSequences>();
            SetSequences = new HashSet<SetSequences>();
        }

        public int DevicePatternId { get; set; }
        public int DeviceId { get; set; }
        public string PatternName { get; set; }

        public Devices Device { get; set; }
        public ICollection<DevicePatternSequences> DevicePatternSequences { get; set; }
        public ICollection<SetSequences> SetSequences { get; set; }
    }
}
