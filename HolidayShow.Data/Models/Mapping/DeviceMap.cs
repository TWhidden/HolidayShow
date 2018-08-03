using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class DeviceMap : EntityTypeConfiguration<Devices>
    {
        public DeviceMap()
        {
            // Primary Key
            this.HasKey(t => t.DeviceId);

            // Properties
            this.Property(t => t.DeviceId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Devices");
            this.Property(t => t.DeviceId).HasColumnName("DeviceId");
            this.Property(t => t.Name).HasColumnName("Name");
        }
    }
}
