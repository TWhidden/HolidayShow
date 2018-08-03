using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class DeviceMap : IEntityTypeConfiguration<Devices>
    {
        public void Configure(EntityTypeBuilder<Devices> builder)
        {
            // Primary Key
            builder.HasKey(t => t.DeviceId);

            // Properties
            builder.Property(t => t.DeviceId)
                .ValueGeneratedNever();

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            builder.ToTable("Devices");
            builder.Property(t => t.DeviceId).HasColumnName("DeviceId");
            builder.Property(t => t.Name).HasColumnName("Name");
        }
    }
}
