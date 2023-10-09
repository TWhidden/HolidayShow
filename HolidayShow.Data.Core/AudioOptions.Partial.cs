namespace HolidayShow.Data.Core
{
    public partial class AudioOptions
    {
        public string DisplayName => $"{Name} - ({AudioDuration/1000}sec / {AudioDuration})ms";
    }
}
