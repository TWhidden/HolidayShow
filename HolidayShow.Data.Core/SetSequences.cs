
using System;

namespace HolidayShow.Data.Core
{
    public partial class SetSequences
    {
        public int SetSequenceId { get; set; }
        public int SetId { get; set; }
        public int OnAt { get; set; }
        public Nullable<int> DevicePatternId { get; set; }
        public Nullable<int> EffectId { get; set; }
    
        public virtual DeviceEffects DeviceEffects { get; set; }
        public virtual Core.DevicePatterns DevicePatterns { get; set; }
        public virtual Sets Sets { get; set; }
    }
}
