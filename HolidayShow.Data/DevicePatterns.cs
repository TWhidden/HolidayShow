//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HolidayShow.Data
{
    using System;
    using System.Collections.Generic;
    
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
