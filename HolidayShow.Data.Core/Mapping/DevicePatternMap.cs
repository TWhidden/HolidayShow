using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class DevicePatternMap : IEntityTypeConfiguration<DevicePatterns>
    {
        public void Configure(EntityTypeBuilder<DevicePatterns> builder)
        {
            // Primary Key
            builder.HasKey(t => t.DevicePatternId);

            // Properties
            builder.Property(t => t.PatternName)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            builder.ToTable("DevicePatterns");
            builder.Property(t => t.DevicePatternId).HasColumnName("DevicePatternId");
            builder.Property(t => t.DeviceId).HasColumnName("DeviceId");
            builder.Property(t => t.PatternName).HasColumnName("PatternName");

            // Relationships
            builder.HasOne(t => t.Devices)
                .WithMany(t => t.DevicePatterns)
                .HasForeignKey(d => d.DeviceId);

        }
    }
}
