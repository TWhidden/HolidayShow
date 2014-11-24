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
    public partial class DeviceIoPorts
    {
        // Place custom code here.

        #region Metadata
        // For more information about how to use the metadata class visit:
        // http://www.plinqo.com/metadata.ashx
        [CodeSmith.Data.Audit.Audit]
        internal class Metadata
        {
             // WARNING: Only attributes inside of this class will be preserved.

            public int DeviceIoPortId { get; set; }

            public int DeviceId { get; set; }

            public int CommandPin { get; set; }

            [Required]
            public string Description { get; set; }

            public bool IsNotVisable { get; set; }

            public bool IsDanger { get; set; }

            public Devices Devices { get; set; }

            public EntitySet<DevicePatternSequences> DevicePatternSequencesList { get; set; }

        }

        #endregion
    }
}