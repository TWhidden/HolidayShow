using System.Configuration;
using HolidayShow.Data.Core.Properties;
using Microsoft.EntityFrameworkCore;

namespace HolidayShow.Data
{
    public class EfHolidayContext : DbContext
    {
        private readonly string _connectionString;

        public EfHolidayContext()
        {
            //_connectionString = "Server=10.64.128.100,1401;Database=HolidayShow_Dev;User Id=dev;Password=dev123;Trusted_Connection=False;";
        }

        public EfHolidayContext(string cs)
        {
            _connectionString = cs;
        }

        public EfHolidayContext(DbContextOptions<EfHolidayContext> options) : base(options)
        {
            
        }

        public void UpdateDatabase()
        {
            //this.Database.ExecuteSqlCommand(Resources.HolidayShow);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AudioOptions>(entity =>
            {
                entity.HasKey(e => e.AudioId);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<DeviceEffects>(entity =>
            {
                entity.HasKey(e => e.EffectId);

                entity.Property(e => e.EffectName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.InstructionMetaData)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.EffectInstructionsAvailable)
                    .WithMany(p => p.DeviceEffects)
                    .HasForeignKey(d => d.EffectInstructionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeviceEffects_EffectInstructionsAvailable");
            });

            modelBuilder.Entity<DeviceIoPorts>(entity =>
            {
                entity.HasKey(e => e.DeviceIoPortId);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.Devices)
                    .WithMany(p => p.DeviceIoPorts)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("FK_DeviceIoPorts_Devices");
            });

            modelBuilder.Entity<DevicePatterns>(entity =>
            {
                entity.HasKey(e => e.DevicePatternId);

                entity.Property(e => e.PatternName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Devices)
                    .WithMany(p => p.DevicePatterns)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("FK_DevicePatterns_Devices");
            });

            modelBuilder.Entity<DevicePatternSequences>(entity =>
            {
                entity.HasKey(e => e.DevicePatternSeqenceId);

                entity.HasOne(d => d.AudioOptions)
                    .WithMany(p => p.DevicePatternSequences)
                    .HasForeignKey(d => d.AudioId)
                    .HasConstraintName("FK_DevicePatternSequences_AudioOptions1");

                entity.HasOne(d => d.DeviceIoPorts)
                    .WithMany(p => p.DevicePatternSequences)
                    .HasForeignKey(d => d.DeviceIoPortId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DevicePatternSequences_DeviceIoPorts");

                entity.HasOne(d => d.DevicePatterns)
                    .WithMany(p => p.DevicePatternSequences)
                    .HasForeignKey(d => d.DevicePatternId)
                    .HasConstraintName("FK_DevicePatternSequences_DevicePatterns");
            });

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.HasKey(e => e.DeviceId);

                entity.Property(e => e.DeviceId).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('NONAME')");
            });

            modelBuilder.Entity<EffectInstructionsAvailable>(entity =>
            {
                entity.HasKey(e => e.EffectInstructionId);

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.InstructionName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.InstructionsForUse)
                    .IsRequired()
                    .HasMaxLength(2000);
            });

            modelBuilder.Entity<Sets>(entity =>
            {
                entity.HasKey(e => e.SetId);

                entity.Property(e => e.SetName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<SetSequences>(entity =>
            {
                entity.HasKey(e => e.SetSequenceId);

                entity.HasOne(d => d.DevicePatterns)
                    .WithMany(p => p.SetSequences)
                    .HasForeignKey(d => d.DevicePatternId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_SetSequences_DevicePatterns");

                entity.HasOne(d => d.DeviceEffects)
                    .WithMany(p => p.SetSequences)
                    .HasForeignKey(d => d.EffectId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_SetSequences_DeviceEffects");

                entity.HasOne(d => d.Sets)
                    .WithMany(p => p.SetSequences)
                    .HasForeignKey(d => d.SetId)
                    .HasConstraintName("FK_SetSequences_Sets");
            });

            modelBuilder.Entity<Settings>(entity =>
            {
                entity.HasKey(e => e.SettingName);

                entity.Property(e => e.SettingName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.ValueString)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Versions>(entity =>
            {
                entity.HasKey(e => e.VersionNumber);

                entity.Property(e => e.VersionNumber).ValueGeneratedNever();

                entity.Property(e => e.DateUpdated).HasColumnType("datetime");
            });
        }


        public virtual Microsoft.EntityFrameworkCore.DbSet<AudioOptions> AudioOptions { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<DeviceIoPorts> DeviceIoPorts { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<DevicePatterns> DevicePatterns { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<DevicePatternSequences> DevicePatternSequences { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<Devices> Devices { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<Sets> Sets { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<Settings> Settings { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<Versions> Versions { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<DeviceEffects> DeviceEffects { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<EffectInstructionsAvailable> EffectInstructionsAvailable { get; set; }
        public virtual Microsoft.EntityFrameworkCore.DbSet<SetSequences> SetSequences { get; set; }
    }
}
