// TECH DEBT
// TO REFACTOR THE TABLE CONFIG INTO INDIVIDUAL CLASS
// SEE TRACKING EVENT TABLE CONFIG FOR EXAMPLE 

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Infrastructure.Database
{
    public class ApplicationDbContext : DbContext
    {
        //linter tells us to use primary constructor syntax, but nahh
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //the tables 
        public virtual DbSet<Parcel> Parcels { get; set; }
        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<ResidentUnit> ResidentUnits { get; set; }

        public virtual DbSet<UserResidentUnit> UserResidentUnits { get; set; }
        
        public virtual DbSet<TrackingEvent> TrackingEvents { get; set; }

        public virtual DbSet<Locker> Lockers { get; set; }

        public virtual DbSet<NotificationPref> NotificationPref { get; set; }

        public virtual DbSet<Session> Sessions { get; set; }

        // system settings table for runtime configuration
        public virtual DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // if we are running unit test, convert any datetimeoffset column into long 
            // because sqlite that we use during unit test does not support DateTimeOffset
            if (Database.IsSqlite())
            {
                foreach (var entity in modelBuilder.Model.GetEntityTypes())
                {
                    var dateTimeOffsetProperties = entity.ClrType.GetProperties()
                        .Where(p => p.PropertyType == typeof(DateTimeOffset) ||
                                p.PropertyType == typeof(DateTimeOffset?));
                    foreach (var property in dateTimeOffsetProperties)
                    {
                        modelBuilder.Entity(entity.Name)
                            .Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter());
                    }
                }
            }

            modelBuilder.ApplyConfiguration(new TrackingEventEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ResidentUnitEntityConfiguration());
            modelBuilder.ApplyConfiguration(new LockerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ParcelEntityConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationPrefEntityConfiguration());

            // making composite primary key for bridge table 
            modelBuilder.Entity<UserResidentUnit>()
                .HasKey(uru => new { uru.UserId, uru.ResidentUnitId });

            modelBuilder.Entity<UserResidentUnit>()
                .HasOne(uru => uru.User)
                .WithMany(u => u.UserResidentUnits)
                .HasForeignKey(uru => uru.UserId);

            modelBuilder.Entity<UserResidentUnit>()
                .HasOne(uru => uru.ResidentUnit)
                .WithMany(ru => ru.UserResidentUnits)
                .HasForeignKey(uru => uru.ResidentUnitId);
        }
    }
}