using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolidayShow.Data
{
    public class SettingKeys
    {
        public const string SetPlaybackOption = "SetPlaybackOption";
        public const string DelayBetweenSets = "SetDelayMs";
        public const string TurnOffAt = "TurnOffAtTime";
        public const string TurnOnAt = "TurnOnAtTime";
        public const string CurrentSet = "CurrentSet";
        public const string OnAt = "TimeOnAt";
        public const string OffAt = "TimeOffAt";
        public const string IsDanagerEnabled = "IsDangerEnabled";
        public const string IsAudioEnabled = "IsAudioEnabled";
    }

    public enum SetPlaybackOptionEnum
    {
        Off = 0,
        PlaybackRandom = 1,
        PlaybackCurrentOnly = 2,
    }

}
