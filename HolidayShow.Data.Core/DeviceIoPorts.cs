using System.Collections.Generic;

namespace HolidayShow.Data.Core
{
    public partial class DeviceIoPorts
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DeviceIoPorts()
        {
            this.DevicePatternSequences = new HashSet<DevicePatternSequences>();
        }
    
        public int DeviceIoPortId { get; set; }
        public int DeviceId { get; set; }
        public int CommandPin { get; set; }
        public string Description { get; set; }
        public bool IsNotVisable { get; set; }
        public bool IsDanger { get; set; }
    
        public virtual Devices Devices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DevicePatternSequences> DevicePatternSequences { get; set; }
    }
}
