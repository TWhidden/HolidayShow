using System.Collections.Generic;

namespace HolidayShow.Data.Core
{
    public partial class DevicePatterns
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DevicePatterns()
        {
            this.DevicePatternSequences = new HashSet<DevicePatternSequences>();
            this.SetSequences = new HashSet<SetSequences>();
        }
    
        public int DevicePatternId { get; set; }
        public int DeviceId { get; set; }
        public string PatternName { get; set; }
    
        public virtual Devices Devices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DevicePatternSequences> DevicePatternSequences { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SetSequences> SetSequences { get; set; }
    }
}
