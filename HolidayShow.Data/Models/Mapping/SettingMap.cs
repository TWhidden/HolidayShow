using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class SettingMap : EntityTypeConfiguration<Settings>
    {
        public SettingMap()
        {
            // Primary Key
            this.HasKey(t => t.SettingName);

            // Properties
            this.Property(t => t.SettingName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.ValueString)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Settings");
            this.Property(t => t.SettingName).HasColumnName("SettingName");
            this.Property(t => t.ValueString).HasColumnName("ValueString");
            this.Property(t => t.ValueDouble).HasColumnName("ValueDouble");
        }
    }
}
