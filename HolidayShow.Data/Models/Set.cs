using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Models
{
    public partial class Set
    {
        public Set()
        {
            this.SetSequences = new List<SetSequence>();
        }

        public int SetId { get; set; }
        public string SetName { get; set; }
        public bool IsDisabled { get; set; }
        public virtual ICollection<SetSequence> SetSequences { get; set; }
    }
}
