using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HolidayShow.Data.Core
{
    public partial class Devices
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Devices()
        {
            this.DeviceIoPorts = new HashSet<DeviceIoPorts>();
            this.DevicePatterns = new HashSet<Core.DevicePatterns>();
        }

        [Required]
        public int DeviceId { get; set; }
        public string Name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DeviceIoPorts> DeviceIoPorts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Core.DevicePatterns> DevicePatterns { get; set; }
    }
}
