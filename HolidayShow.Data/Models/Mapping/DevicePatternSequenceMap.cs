using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class DevicePatternSequenceMap : EntityTypeConfiguration<DevicePatternSequences>
    {
        public DevicePatternSequenceMap()
        {
            // Primary Key
            this.HasKey(t => t.DevicePatternSeqenceId);

            // Properties
            // Table & Column Mappings
            this.ToTable("DevicePatternSequences");
            this.Property(t => t.DevicePatternSeqenceId).HasColumnName("DevicePatternSeqenceId");
            this.Property(t => t.DevicePatternId).HasColumnName("DevicePatternId");
            this.Property(t => t.OnAt).HasColumnName("OnAt");
            this.Property(t => t.Duration).HasColumnName("Duration");
            this.Property(t => t.AudioId).HasColumnName("AudioId");
            this.Property(t => t.DeviceIoPortId).HasColumnName("DeviceIoPortId");

            // Relationships
            this.HasRequired(t => t.AudioOptions)
                .WithMany(t => t.DevicePatternSequences)
                .HasForeignKey(d => d.AudioId);
            this.HasRequired(t => t.DeviceIoPorts)
                .WithMany(t => t.DevicePatternSequences)
                .HasForeignKey(d => d.DeviceIoPortId);
            this.HasRequired(t => t.DevicePatterns)
                .WithMany(t => t.DevicePatternSequences)
                .HasForeignKey(d => d.DevicePatternId);

        }
    }
}
