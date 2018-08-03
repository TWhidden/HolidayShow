using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Models
{
    public partial class SetSequence
    {
        public int SetSequenceId { get; set; }
        public int SetId { get; set; }
        public int OnAt { get; set; }
        public Nullable<int> DevicePatternId { get; set; }
        public Nullable<int> EffectId { get; set; }
        public virtual DeviceEffect DeviceEffect { get; set; }
        public virtual DevicePattern DevicePattern { get; set; }
        public virtual Set Set { get; set; }
    }
}
