using System.ComponentModel.DataAnnotations;

namespace HolidayShow.Data.Core
{
    public partial class Settings
    {
        [Required]
        public string SettingName { get; set; }
        public string ValueString { get; set; } = string.Empty;
        public double ValueDouble { get; set; }
    }
}
