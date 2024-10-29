using System.ComponentModel.DataAnnotations;

namespace HolidayShow.Data.Core
{
    public partial class DevicePatternSequences
    {
        [Required]
        public int DevicePatternSeqenceId { get; set; }
        public int DevicePatternId { get; set; }
        [Required]
        public int OnAt { get; set; }
        public int Duration { get; set; }
        public int AudioId { get; set; }
        public int DeviceIoPortId { get; set; }
    
        public virtual Core.AudioOptions AudioOptions { get; set; }
        public virtual DeviceIoPorts DeviceIoPorts { get; set; }
        public virtual Core.DevicePatterns DevicePatterns { get; set; }
    }
}
