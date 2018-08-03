using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class DeviceEffectMap : IEntityTypeConfiguration<DeviceEffects>
    {
        public void Configure(EntityTypeBuilder<DeviceEffects> builder)
        {
            // Primary Key
            builder.HasKey(t => t.EffectId);

            // Properties
            builder.Property(t => t.EffectName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.InstructionMetaData)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            builder.ToTable("DeviceEffects");
            builder.Property(t => t.EffectId).HasColumnName("EffectId");
            builder.Property(t => t.EffectName).HasColumnName("EffectName");
            builder.Property(t => t.InstructionMetaData).HasColumnName("InstructionMetaData");
            builder.Property(t => t.Duration).HasColumnName("Duration");
            builder.Property(t => t.EffectInstructionId).HasColumnName("EffectInstructionId");

            // Relationships
            builder.HasOne(t => t.EffectInstructionsAvailable)
                .WithMany(t => t.DeviceEffects)
                .HasForeignKey(d => d.EffectInstructionId);

        }
    }
}
