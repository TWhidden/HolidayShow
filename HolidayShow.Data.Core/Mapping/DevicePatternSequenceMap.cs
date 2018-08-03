using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class DevicePatternSequenceMap : IEntityTypeConfiguration<DevicePatternSequences>
    {
        public void Configure(EntityTypeBuilder<DevicePatternSequences> builder)
        {
            // Primary Key
            builder.HasKey(t => t.DevicePatternSeqenceId);

            // Properties
            // Table & Column Mappings
            builder.ToTable("DevicePatternSequences");
            builder.Property(t => t.DevicePatternSeqenceId).HasColumnName("DevicePatternSeqenceId");
            builder.Property(t => t.DevicePatternId).HasColumnName("DevicePatternId");
            builder.Property(t => t.OnAt).HasColumnName("OnAt");
            builder.Property(t => t.Duration).HasColumnName("Duration");
            builder.Property(t => t.AudioId).HasColumnName("AudioId");
            builder.Property(t => t.DeviceIoPortId).HasColumnName("DeviceIoPortId");

            // Relationships
            builder.HasOne(t => t.AudioOptions)
                .WithMany(t => t.DevicePatternSequences)
                .HasForeignKey(d => d.AudioId);
            builder.HasOne(t => t.DeviceIoPorts)
                .WithMany(t => t.DevicePatternSequences)
                .HasForeignKey(d => d.DeviceIoPortId);
            builder.HasOne(t => t.DevicePatterns)
                .WithMany(t => t.DevicePatternSequences)
                .HasForeignKey(d => d.DevicePatternId);

        }
    }
}
