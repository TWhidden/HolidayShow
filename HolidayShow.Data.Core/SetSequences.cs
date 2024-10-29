
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HolidayShow.Data.Core
{
    public partial class SetSequences
    {
        [Required]
        public int SetSequenceId { get; set; }
        
        public int SetId { get; set; }
        [Required]
        public int OnAt { get; set; }
        public int? DevicePatternId { get; set; }
        public int? EffectId { get; set; }

        [JsonIgnore]
        public virtual DeviceEffects DeviceEffects { get; set; }

        [JsonIgnore]
        public virtual Core.DevicePatterns DevicePatterns { get; set; }

        [JsonIgnore]
        public virtual Sets Sets { get; set; }
    }
}
