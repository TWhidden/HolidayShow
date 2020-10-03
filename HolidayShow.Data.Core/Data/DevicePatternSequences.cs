using System;
using System.Collections.Generic;

namespace HolidayShow.Data.Core.Data
{
    public partial class DevicePatternSequences
    {
        public int DevicePatternSeqenceId { get; set; }
        public int DevicePatternId { get; set; }
        public int OnAt { get; set; }
        public int Duration { get; set; }
        public int AudioId { get; set; }
        public int DeviceIoPortId { get; set; }

        public AudioOptions Audio { get; set; }
        public DeviceIoPorts DeviceIoPort { get; set; }
        public DevicePatterns DevicePattern { get; set; }
    }
}
