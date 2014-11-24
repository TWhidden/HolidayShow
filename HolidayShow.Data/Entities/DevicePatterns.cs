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
    public partial class DevicePatterns
    {
        // Place custom code here.

        #region Metadata
        // For more information about how to use the metadata class visit:
        // http://www.plinqo.com/metadata.ashx
        [CodeSmith.Data.Audit.Audit]
        internal class Metadata
        {
             // WARNING: Only attributes inside of this class will be preserved.

            public int DevicePatternId { get; set; }

            public int DeviceId { get; set; }

            [Required]
            public string PatternName { get; set; }

            public Devices Devices { get; set; }

            public EntitySet<DevicePatternSequences> DevicePatternSequencesList { get; set; }

            public EntitySet<SetSequences> SetSequencesList { get; set; }

        }

        #endregion

        public string PatternDescription
        {
            get { return string.Format("{0} - {1}", Devices.Name, PatternName); }
        }
    }
}