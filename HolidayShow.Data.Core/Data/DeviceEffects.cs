using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Core.Data
{
    public partial class DeviceEffects
    {
        public DeviceEffects()
        {
            SetSequences = new HashSet<SetSequences>();
        }

        public int EffectId { get; set; }
        public string EffectName { get; set; }
        public string InstructionMetaData { get; set; }
        public int Duration { get; set; }
        public int EffectInstructionId { get; set; }
        public string TimeOn { get; set; }
        public string TimeOff { get; set; }

        public EffectInstructionsAvailable EffectInstruction { get; set; }
        public ICollection<SetSequences> SetSequences { get; set; }
    }
}
