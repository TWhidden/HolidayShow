using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class AudioOptionMap : IEntityTypeConfiguration<AudioOptions>
    {
        public void Configure(EntityTypeBuilder<AudioOptions> builder)
        {
            // Primary Key
            builder.HasKey(t => t.AudioId);

            // Properties
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.FileName)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            builder.ToTable("AudioOptions");
            builder.Property(t => t.AudioId).HasColumnName("AudioId");
            builder.Property(t => t.Name).HasColumnName("Name");
            builder.Property(t => t.FileName).HasColumnName("FileName");
            builder.Property(t => t.AudioDuration).HasColumnName("AudioDuration");
            builder.Property(t => t.IsNotVisable).HasColumnName("IsNotVisable");
        }
    }
}
