using System.Collections.Generic;

namespace HolidayShow.Data.Core
{
    public partial class Sets
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Sets()
        {
            this.SetSequences = new HashSet<SetSequences>();
        }
    
        public int SetId { get; set; }
        public string SetName { get; set; }
        public bool IsDisabled { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SetSequences> SetSequences { get; set; }
    }
}
