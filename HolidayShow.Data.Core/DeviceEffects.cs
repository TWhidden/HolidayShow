using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HolidayShow.Data.Core
{
    public partial class DeviceEffects
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DeviceEffects()
        {
            this.SetSequences = new HashSet<SetSequences>();
        }

        [Required]
        public int EffectId { get; set; }
        public string EffectName { get; set; }
        public string InstructionMetaData { get; set; }
        public int Duration { get; set; }
        [Required]
        public int EffectInstructionId { get; set; }
        public string TimeOn { get; set; }
        public string TimeOff { get; set; }
    
        public virtual EffectInstructionsAvailable EffectInstructionsAvailable { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SetSequences> SetSequences { get; set; }
    }
}
