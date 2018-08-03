using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Models
{
    public partial class DevicePatternSequence
    {
        public int DevicePatternSeqenceId { get; set; }
        public int DevicePatternId { get; set; }
        public int OnAt { get; set; }
        public int Duration { get; set; }
        public int AudioId { get; set; }
        public int DeviceIoPortId { get; set; }
        public virtual AudioOption AudioOption { get; set; }
        public virtual DeviceIoPort DeviceIoPort { get; set; }
        public virtual DevicePattern DevicePattern { get; set; }
    }
}
