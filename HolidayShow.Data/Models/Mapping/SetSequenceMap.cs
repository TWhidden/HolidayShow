using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class SetSequenceMap : EntityTypeConfiguration<SetSequences>
    {
        public SetSequenceMap()
        {
            // Primary Key
            this.HasKey(t => t.SetSequenceId);

            // Properties
            // Table & Column Mappings
            this.ToTable("SetSequences");
            this.Property(t => t.SetSequenceId).HasColumnName("SetSequenceId");
            this.Property(t => t.SetId).HasColumnName("SetId");
            this.Property(t => t.OnAt).HasColumnName("OnAt");
            this.Property(t => t.DevicePatternId).HasColumnName("DevicePatternId");
            this.Property(t => t.EffectId).HasColumnName("EffectId");

            // Relationships
            this.HasOptional(t => t.DeviceEffects)
                .WithMany(t => t.SetSequences)
                .HasForeignKey(d => d.EffectId);
            this.HasOptional(t => t.DevicePatterns)
                .WithMany(t => t.SetSequences)
                .HasForeignKey(d => d.DevicePatternId);
            this.HasRequired(t => t.Sets)
                .WithMany(t => t.SetSequences)
                .HasForeignKey(d => d.SetId);

        }
    }
}
