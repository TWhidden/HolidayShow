using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using HolidayShow.Data.Models.Mapping;

namespace HolidayShow.Data.Models
{
    public partial class HolidayShowContext : DbContext
    {
        static HolidayShowContext()
        {
            Database.SetInitializer<HolidayShowContext>(null);
        }

        public HolidayShowContext()
            : base("Name=HolidayShowContext")
        {
        }

        public DbSet<AudioOption> AudioOptions { get; set; }
        public DbSet<DeviceEffect> DeviceEffects { get; set; }
        public DbSet<DeviceIoPort> DeviceIoPorts { get; set; }
        public DbSet<DevicePattern> DevicePatterns { get; set; }
        public DbSet<DevicePatternSequence> DevicePatternSequences { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<EffectInstructionsAvailable> EffectInstructionsAvailables { get; set; }
        public DbSet<Set> Sets { get; set; }
        public DbSet<SetSequence> SetSequences { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Version> Versions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new AudioOptionMap());
            modelBuilder.Configurations.Add(new DeviceEffectMap());
            modelBuilder.Configurations.Add(new DeviceIoPortMap());
            modelBuilder.Configurations.Add(new DevicePatternMap());
            modelBuilder.Configurations.Add(new DevicePatternSequenceMap());
            modelBuilder.Configurations.Add(new DeviceMap());
            modelBuilder.Configurations.Add(new EffectInstructionsAvailableMap());
            modelBuilder.Configurations.Add(new SetMap());
            modelBuilder.Configurations.Add(new SetSequenceMap());
            modelBuilder.Configurations.Add(new SettingMap());
            modelBuilder.Configurations.Add(new VersionMap());
        }
    }
}
