using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Models
{
    public partial class AudioOption
    {
        public AudioOption()
        {
            this.DevicePatternSequences = new List<DevicePatternSequence>();
        }

        public int AudioId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public int AudioDuration { get; set; }
        public bool IsNotVisable { get; set; }
        public virtual ICollection<DevicePatternSequence> DevicePatternSequences { get; set; }
    }
}
