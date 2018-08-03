using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class AudioOptionMap : EntityTypeConfiguration<AudioOptions>
    {
        public AudioOptionMap()
        {
            // Primary Key
            this.HasKey(t => t.AudioId);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.FileName)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("AudioOptions");
            this.Property(t => t.AudioId).HasColumnName("AudioId");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.FileName).HasColumnName("FileName");
            this.Property(t => t.AudioDuration).HasColumnName("AudioDuration");
            this.Property(t => t.IsNotVisable).HasColumnName("IsNotVisable");
        }
    }
}
