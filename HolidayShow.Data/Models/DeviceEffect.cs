using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Models
{
    public partial class DeviceEffect
    {
        public DeviceEffect()
        {
            this.SetSequences = new List<SetSequence>();
        }

        public int EffectId { get; set; }
        public string EffectName { get; set; }
        public string InstructionMetaData { get; set; }
        public int Duration { get; set; }
        public int EffectInstructionId { get; set; }
        public virtual EffectInstructionsAvailable EffectInstructionsAvailable { get; set; }
        public virtual ICollection<SetSequence> SetSequences { get; set; }
    }
}
