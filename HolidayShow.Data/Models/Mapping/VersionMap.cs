using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class VersionMap : EntityTypeConfiguration<Versions>
    {
        public VersionMap()
        {
            // Primary Key
            this.HasKey(t => t.VersionNumber);

            // Properties
            this.Property(t => t.VersionNumber)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("Versions");
            this.Property(t => t.VersionNumber).HasColumnName("VersionNumber");
            this.Property(t => t.DateUpdated).HasColumnName("DateUpdated");
        }
    }
}
