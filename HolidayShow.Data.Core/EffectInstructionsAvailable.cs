using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HolidayShow.Data.Core
{
    public partial class EffectInstructionsAvailable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EffectInstructionsAvailable()
        {
            this.DeviceEffects = new HashSet<DeviceEffects>();
        }

        [Required]
        public int EffectInstructionId { get; set; }
        public string DisplayName { get; set; }
        public string InstructionName { get; set; }
        public string InstructionsForUse { get; set; }
        public bool IsDisabled { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DeviceEffects> DeviceEffects { get; set; }
    }
}
