
namespace HolidayShow.Data
{
    public partial class EfHolidayContext
    {
        public void UpdateDatabase()
        {
            this.Database.ExecuteSqlCommand(HolidayShow.Data.Properties.Resources.HolidayShow);
        }
    }
}
