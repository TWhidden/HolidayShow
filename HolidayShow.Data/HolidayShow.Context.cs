﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HolidayShow.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EfHolidayContext : DbContext
    {
        public EfHolidayContext()
            : base("name=EfHolidayContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AudioOptions> AudioOptions { get; set; }
        public virtual DbSet<DeviceEffects> DeviceEffects { get; set; }
        public virtual DbSet<DeviceIoPorts> DeviceIoPorts { get; set; }
        public virtual DbSet<DevicePatterns> DevicePatterns { get; set; }
        public virtual DbSet<DevicePatternSequences> DevicePatternSequences { get; set; }
        public virtual DbSet<Devices> Devices { get; set; }
        public virtual DbSet<Sets> Sets { get; set; }
        public virtual DbSet<SetSequences> SetSequences { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<Versions> Versions { get; set; }
    }
}
