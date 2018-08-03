using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class DeviceEffectMap : EntityTypeConfiguration<DeviceEffects>
    {
        public DeviceEffectMap()
        {
            // Primary Key
            this.HasKey(t => t.EffectId);

            // Properties
            this.Property(t => t.EffectName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.InstructionMetaData)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("DeviceEffects");
            this.Property(t => t.EffectId).HasColumnName("EffectId");
            this.Property(t => t.EffectName).HasColumnName("EffectName");
            this.Property(t => t.InstructionMetaData).HasColumnName("InstructionMetaData");
            this.Property(t => t.Duration).HasColumnName("Duration");
            this.Property(t => t.EffectInstructionId).HasColumnName("EffectInstructionId");

            // Relationships
            this.HasRequired(t => t.EffectInstructionsAvailable)
                .WithMany(t => t.DeviceEffects)
                .HasForeignKey(d => d.EffectInstructionId);

        }
    }
}
