using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace HolidayShow.Data.Models.Mapping
{
    public class SetMap : EntityTypeConfiguration<Sets>
    {
        public SetMap()
        {
            // Primary Key
            this.HasKey(t => t.SetId);

            // Properties
            this.Property(t => t.SetName)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Sets");
            this.Property(t => t.SetId).HasColumnName("SetId");
            this.Property(t => t.SetName).HasColumnName("SetName");
            this.Property(t => t.IsDisabled).HasColumnName("IsDisabled");
        }
    }
}
