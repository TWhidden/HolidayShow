
using System.Data.Entity.Core.EntityClient;

namespace HolidayShow.Data
{
    public partial class EfHolidayContext
    {
        public void UpdateDatabase()
        {
            this.Database.ExecuteSqlCommand(Properties.Resources.HolidayShow);
        }

        public EfHolidayContext(string connectionString)
        {
            var entityBuilder = new EntityConnectionStringBuilder
            {
                ProviderConnectionString = connectionString,
                Metadata = @"res://*/HolidayShow.csdl|res://*/HolidayShow.ssdl|res://*/HolidayShow.msl"
            };

            base.Database.Connection.ConnectionString = entityBuilder.ConnectionString;
        }

    }
}
