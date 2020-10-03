using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Core.Data
{
    public partial class SetSequences
    {
        public int SetSequenceId { get; set; }
        public int SetId { get; set; }
        public int OnAt { get; set; }
        public int? DevicePatternId { get; set; }
        public int? EffectId { get; set; }

        public DevicePatterns DevicePattern { get; set; }
        public DeviceEffects Effect { get; set; }
        public Sets Set { get; set; }
    }
}
