using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Models
{
    public partial class EffectInstructionsAvailable
    {
        public EffectInstructionsAvailable()
        {
            this.DeviceEffects = new List<DeviceEffect>();
        }

        public int EffectInstructionId { get; set; }
        public string DisplayName { get; set; }
        public string InstructionName { get; set; }
        public string InstructionsForUse { get; set; }
        public bool IsDisabled { get; set; }
        public virtual ICollection<DeviceEffect> DeviceEffects { get; set; }
    }
}
