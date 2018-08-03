using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class SetSequenceMap : IEntityTypeConfiguration<SetSequences>
    {
        public void Configure(EntityTypeBuilder<SetSequences> builder)
        {
            // Primary Key
            builder.HasKey(t => t.SetSequenceId);

            // Properties
            // Table & Column Mappings
            builder.ToTable("SetSequences");
            builder.Property(t => t.SetSequenceId).HasColumnName("SetSequenceId");
            builder.Property(t => t.SetId).HasColumnName("SetId");
            builder.Property(t => t.OnAt).HasColumnName("OnAt");
            builder.Property(t => t.DevicePatternId).HasColumnName("DevicePatternId");
            builder.Property(t => t.EffectId).HasColumnName("EffectId");

            // Relationships
            //builder.HasOptional(t => t.DeviceEffects)
            //    .WithMany(t => t.SetSequences)
            //    .HasForeignKey(d => d.EffectId);
            //builder.HasOptional(t => t.DevicePatterns)
            //    .WithMany(t => t.SetSequences)
            //    .HasForeignKey(d => d.DevicePatternId);
            builder.HasOne(t => t.Sets)
                .WithMany(t => t.SetSequences)
                .HasForeignKey(d => d.SetId);

        }
    }
}
