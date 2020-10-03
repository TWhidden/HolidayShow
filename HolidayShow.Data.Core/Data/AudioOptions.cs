using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Core.Data
{
    public partial class AudioOptions
    {
        public AudioOptions()
        {
            DevicePatternSequences = new HashSet<DevicePatternSequences>();
        }

        public int AudioId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public int AudioDuration { get; set; }
        public bool IsNotVisable { get; set; }

        public ICollection<DevicePatternSequences> DevicePatternSequences { get; set; }
    }
}
