using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class DeviceIoPortMap : IEntityTypeConfiguration<DeviceIoPorts>
    {
        public void Configure(EntityTypeBuilder<DeviceIoPorts> builder)
        {
            // Primary Key
            builder.HasKey(t => t.DeviceIoPortId);

            // Properties
            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            builder.ToTable("DeviceIoPorts");
            builder.Property(t => t.DeviceIoPortId).HasColumnName("DeviceIoPortId");
            builder.Property(t => t.DeviceId).HasColumnName("DeviceId");
            builder.Property(t => t.CommandPin).HasColumnName("CommandPin");
            builder.Property(t => t.Description).HasColumnName("Description");
            builder.Property(t => t.IsNotVisable).HasColumnName("IsNotVisable");
            builder.Property(t => t.IsDanger).HasColumnName("IsDanger");

            // Relationships
            builder.HasOne(t => t.Devices)
                .WithMany(t => t.DeviceIoPorts)
                .HasForeignKey(d => d.DeviceId);

        }
    }
}
