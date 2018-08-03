using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class SetMap : IEntityTypeConfiguration<Sets>
    {
        public void Configure(EntityTypeBuilder<Sets> builder)
        {
            // Primary Key
            builder.HasKey(t => t.SetId);

            // Properties
            builder.Property(t => t.SetName)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            builder.ToTable("Sets");
            builder.Property(t => t.SetId).HasColumnName("SetId");
            builder.Property(t => t.SetName).HasColumnName("SetName");
            builder.Property(t => t.IsDisabled).HasColumnName("IsDisabled");
        }
    }
}
