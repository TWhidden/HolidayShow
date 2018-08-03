using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class DeviceIoPortMap : EntityTypeConfiguration<DeviceIoPorts>
    {
        public DeviceIoPortMap()
        {
            // Primary Key
            this.HasKey(t => t.DeviceIoPortId);

            // Properties
            this.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("DeviceIoPorts");
            this.Property(t => t.DeviceIoPortId).HasColumnName("DeviceIoPortId");
            this.Property(t => t.DeviceId).HasColumnName("DeviceId");
            this.Property(t => t.CommandPin).HasColumnName("CommandPin");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.IsNotVisable).HasColumnName("IsNotVisable");
            this.Property(t => t.IsDanger).HasColumnName("IsDanger");

            // Relationships
            this.HasRequired(t => t.Devices)
                .WithMany(t => t.DeviceIoPorts)
                .HasForeignKey(d => d.DeviceId);

        }
    }
}
