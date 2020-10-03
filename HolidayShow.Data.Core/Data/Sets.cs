using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Core.Data
{
    public partial class Sets
    {
        public Sets()
        {
            SetSequences = new HashSet<SetSequences>();
        }

        public int SetId { get; set; }
        public string SetName { get; set; }
        public bool IsDisabled { get; set; }

        public ICollection<SetSequences> SetSequences { get; set; }
    }
}
