using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class EffectInstructionsAvailableMap : IEntityTypeConfiguration<EffectInstructionsAvailable>
    {
        public void Configure(EntityTypeBuilder<EffectInstructionsAvailable> builder)
        {
            // Primary Key
            builder.HasKey(t => t.EffectInstructionId);

            // Properties
            builder.Property(t => t.DisplayName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.InstructionName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.InstructionsForUse)
                .IsRequired()
                .HasMaxLength(2000);

            // Table & Column Mappings
            builder.ToTable("EffectInstructionsAvailable");
            builder.Property(t => t.EffectInstructionId).HasColumnName("EffectInstructionId");
            builder.Property(t => t.DisplayName).HasColumnName("DisplayName");
            builder.Property(t => t.InstructionName).HasColumnName("InstructionName");
            builder.Property(t => t.InstructionsForUse).HasColumnName("InstructionsForUse");
            builder.Property(t => t.IsDisabled).HasColumnName("IsDisabled");
        }
    }
}
