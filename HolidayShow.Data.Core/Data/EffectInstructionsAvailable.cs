using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Core.Data
{
    public partial class EffectInstructionsAvailable
    {
        public EffectInstructionsAvailable()
        {
            DeviceEffects = new HashSet<DeviceEffects>();
        }

        public int EffectInstructionId { get; set; }
        public string DisplayName { get; set; }
        public string InstructionName { get; set; }
        public string InstructionsForUse { get; set; }
        public bool IsDisabled { get; set; }

        public ICollection<DeviceEffects> DeviceEffects { get; set; }
    }
}
