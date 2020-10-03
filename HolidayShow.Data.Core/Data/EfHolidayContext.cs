using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HolidayShow.Data.Core.Data
{
    public partial class EfHolidayContext : DbContext
    {
        public EfHolidayContext()
        {
        }

        public EfHolidayContext(DbContextOptions<EfHolidayContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AudioOptions> AudioOptions { get; set; }
        public virtual DbSet<DeviceEffects> DeviceEffects { get; set; }
        public virtual DbSet<DeviceIoPorts> DeviceIoPorts { get; set; }
        public virtual DbSet<DevicePatterns> DevicePatterns { get; set; }
        public virtual DbSet<DevicePatternSequences> DevicePatternSequences { get; set; }
        public virtual DbSet<Devices> Devices { get; set; }
        public virtual DbSet<EffectInstructionsAvailable> EffectInstructionsAvailable { get; set; }
        public virtual DbSet<Sets> Sets { get; set; }
        public virtual DbSet<SetSequences> SetSequences { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<Versions> Versions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=10.64.128.100,1401;Database=HolidayShow_Dev;User Id=dev;Password=dev123;Trusted_Connection=False;");
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

                entity.Property(e => e.TimeOff)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.TimeOn)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.EffectInstruction)
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

                entity.HasOne(d => d.Device)
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

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.DevicePatterns)
                    .HasForeignKey(d => d.DeviceId)
                    .HasConstraintName("FK_DevicePatterns_Devices");
            });

            modelBuilder.Entity<DevicePatternSequences>(entity =>
            {
                entity.HasKey(e => e.DevicePatternSeqenceId);

                entity.HasOne(d => d.Audio)
                    .WithMany(p => p.DevicePatternSequences)
                    .HasForeignKey(d => d.AudioId)
                    .HasConstraintName("FK_DevicePatternSequences_AudioOptions1");

                entity.HasOne(d => d.DeviceIoPort)
                    .WithMany(p => p.DevicePatternSequences)
                    .HasForeignKey(d => d.DeviceIoPortId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DevicePatternSequences_DeviceIoPorts");

                entity.HasOne(d => d.DevicePattern)
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

                entity.HasOne(d => d.DevicePattern)
                    .WithMany(p => p.SetSequences)
                    .HasForeignKey(d => d.DevicePatternId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_SetSequences_DevicePatterns");

                entity.HasOne(d => d.Effect)
                    .WithMany(p => p.SetSequences)
                    .HasForeignKey(d => d.EffectId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_SetSequences_DeviceEffects");

                entity.HasOne(d => d.Set)
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
    }
}
