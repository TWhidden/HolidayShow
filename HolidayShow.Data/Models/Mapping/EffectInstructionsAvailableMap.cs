using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class EffectInstructionsAvailableMap : EntityTypeConfiguration<EffectInstructionsAvailable>
    {
        public EffectInstructionsAvailableMap()
        {
            // Primary Key
            this.HasKey(t => t.EffectInstructionId);

            // Properties
            this.Property(t => t.DisplayName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.InstructionName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.InstructionsForUse)
                .IsRequired()
                .HasMaxLength(2000);

            // Table & Column Mappings
            this.ToTable("EffectInstructionsAvailable");
            this.Property(t => t.EffectInstructionId).HasColumnName("EffectInstructionId");
            this.Property(t => t.DisplayName).HasColumnName("DisplayName");
            this.Property(t => t.InstructionName).HasColumnName("InstructionName");
            this.Property(t => t.InstructionsForUse).HasColumnName("InstructionsForUse");
            this.Property(t => t.IsDisabled).HasColumnName("IsDisabled");
        }
    }
}
