using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class VersionMap : IEntityTypeConfiguration<Versions>
    {
        public void Configure(EntityTypeBuilder<Versions> builder)
        {
            // Primary Key
            builder.HasKey(t => t.VersionNumber);

            // Properties
            builder.Property(t => t.VersionNumber)
                .ValueGeneratedNever();

            // Table & Column Mappings
            builder.ToTable("Versions");
            builder.Property(t => t.VersionNumber).HasColumnName("VersionNumber");
            builder.Property(t => t.DateUpdated).HasColumnName("DateUpdated");
        }
    }
}
