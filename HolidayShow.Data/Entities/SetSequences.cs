using System;
using System.Linq;
using System.Data.Linq;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CodeSmith.Data.Attributes;
using CodeSmith.Data.Rules;

namespace HolidayShow.Data
{
    public partial class SetSequences
    {
        // Place custom code here.

        #region Metadata
        // For more information about how to use the metadata class visit:
        // http://www.plinqo.com/metadata.ashx
        [CodeSmith.Data.Audit.Audit]
        internal class Metadata
        {
             // WARNING: Only attributes inside of this class will be preserved.

            public int SetId { get; set; }

            public int OnAt { get; set; }

            public int DevicePatternId { get; set; }

            public int SetSequenceId { get; set; }

            public DevicePatterns DevicePatterns { get; set; }

            public Sets Sets { get; set; }

        }

        #endregion
    }
}