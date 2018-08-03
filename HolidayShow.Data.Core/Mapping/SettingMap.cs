using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HolidayShow.Data.Models.Mapping
{
    public class SettingMap : IEntityTypeConfiguration<Settings>
    {
        public void Configure(EntityTypeBuilder<Settings> builder)
        {
            // Primary Key
            builder.HasKey(t => t.SettingName);

            // Properties
            builder.Property(t => t.SettingName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.ValueString)
                .IsRequired();

            // Table & Column Mappings
            builder.ToTable("Settings");
            builder.Property(t => t.SettingName).HasColumnName("SettingName");
            builder.Property(t => t.ValueString).HasColumnName("ValueString");
            builder.Property(t => t.ValueDouble).HasColumnName("ValueDouble");
        }
    }
}
