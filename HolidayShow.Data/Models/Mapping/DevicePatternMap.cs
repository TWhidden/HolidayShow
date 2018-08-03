using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class DevicePatternMap : EntityTypeConfiguration<DevicePatterns>
    {
        public DevicePatternMap()
        {
            // Primary Key
            this.HasKey(t => t.DevicePatternId);

            // Properties
            this.Property(t => t.PatternName)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("DevicePatterns");
            this.Property(t => t.DevicePatternId).HasColumnName("DevicePatternId");
            this.Property(t => t.DeviceId).HasColumnName("DeviceId");
            this.Property(t => t.PatternName).HasColumnName("PatternName");

            // Relationships
            this.HasRequired(t => t.Devices)
                .WithMany(t => t.DevicePatterns)
                .HasForeignKey(d => d.DeviceId);

        }
    }
}
