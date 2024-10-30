using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [Required]
        public int DevicePatternId { get; set; }
        public int DeviceId { get; set; }
        public string PatternName { get; set; }

        [JsonIgnore]
        public virtual Devices Devices { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DevicePatternSequences> DevicePatternSequences { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SetSequences> SetSequences { get; set; }
    }
}
