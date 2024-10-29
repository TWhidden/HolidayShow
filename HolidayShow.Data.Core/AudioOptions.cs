using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HolidayShow.Data.Core
{
    public partial class AudioOptions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AudioOptions()
        {
            this.DevicePatternSequences = new HashSet<DevicePatternSequences>();
        }

        [Required]
        public int AudioId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public int AudioDuration { get; set; }
        public bool IsNotVisable { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DevicePatternSequences> DevicePatternSequences { get; set; }
    }
}
