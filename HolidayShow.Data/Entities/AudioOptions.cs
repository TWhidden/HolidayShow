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
    public partial class AudioOptions
    {
        // Place custom code here.

        #region Metadata
        // For more information about how to use the metadata class visit:
        // http://www.plinqo.com/metadata.ashx
        [CodeSmith.Data.Audit.Audit]
        internal class Metadata
        {
             // WARNING: Only attributes inside of this class will be preserved.

            public int AudioId { get; set; }

            [Required]
            public string Name { get; set; }

            public string FileName { get; set; }

            public int AudioDuration { get; set; }

            public bool IsNotVisable { get; set; }

            public EntitySet<DevicePatternSequences> DevicePatternSequencesList { get; set; }

        }

        #endregion

        public string DisplayName
        {
            get { return string.Format("{0} - ({1}sec / {2})ms", Name, AudioDuration/1000, AudioDuration); }
        }
    }
}