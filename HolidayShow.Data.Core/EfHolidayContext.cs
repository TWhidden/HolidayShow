using System;
using System.Data.Entity;
using HolidayShow.Data.Models.Mapping;
using Microsoft.EntityFrameworkCore;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace HolidayShow.Data.Core
{
    public class EfHolidayContext : DbContext
    {
        private readonly string _connectionString;

        public EfHolidayContext(string cs)
        {
            _connectionString = cs;
        }

        public void UpdateDatabase()
        {
            //this.Database.ExecuteSqlCommand(Properties.Resources.HolidayShow);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AudioOptions>().HasKey(x => x.AudioId);
            modelBuilder.Entity<DeviceIoPorts>().HasKey(x => x.DeviceIoPortId);
            modelBuilder.Entity<DevicePatterns>().HasKey(x => x.DevicePatternId);
            modelBuilder.Entity<DevicePatternSequences>().HasKey(x => x.DevicePatternSeqenceId);
            modelBuilder.Entity<Devices>().HasKey(x => x.DeviceId);
            modelBuilder.Entity<Sets>().HasKey(x => x.SetId);
            modelBuilder.Entity<Settings>().HasKey(x => x.SettingName);
            modelBuilder.Entity<Versions>().HasKey(x => x.VersionNumber);
            modelBuilder.Entity<DeviceEffects>().HasKey(x => x.EffectId);
            modelBuilder.Entity<EffectInstructionsAvailable>().HasKey(x => x.EffectInstructionId);
            modelBuilder.Entity<SetSequences>().HasKey(x => x.SetSequenceId);

            modelBuilder.ApplyConfiguration(new AudioOptionMap());
            modelBuilder.ApplyConfiguration(new DeviceEffectMap());
            modelBuilder.ApplyConfiguration(new DeviceIoPortMap());
            modelBuilder.ApplyConfiguration(new DevicePatternMap());
            modelBuilder.ApplyConfiguration(new DevicePatternSequenceMap());
            modelBuilder.ApplyConfiguration(new DeviceMap());
            modelBuilder.ApplyConfiguration(new EffectInstructionsAvailableMap());
            modelBuilder.ApplyConfiguration(new SetMap());
            modelBuilder.ApplyConfiguration(new SetSequenceMap());
            modelBuilder.ApplyConfiguration(new SettingMap());
            modelBuilder.ApplyConfiguration(new VersionMap());
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
