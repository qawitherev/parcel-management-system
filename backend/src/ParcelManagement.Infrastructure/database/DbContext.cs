// TECH DEBT
// TO REFACTOR THE TABLE CONFIG INTO INDIVIDUAL CLASS
// SEE TRACKING EVENT TABLE CONFIG FOR EXAMPLE 

using Microsoft.EntityFrameworkCore;
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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TrackingEventEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ResidentUnitEntityConfiguration());
            modelBuilder.ApplyConfiguration(new LockerEntityConfiguration());

            // configure the column to use string enum instead of int
            modelBuilder.Entity<Parcel>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Parcel>()
                .Property(p => p.Weight)
                .HasPrecision(18, 2);

            // add unique constraint on tracking number and index it
            modelBuilder.Entity<Parcel>()
                .HasIndex(p => p.TrackingNumber)
                .IsUnique();

            // it is a best practice to start from the dependent entity—the one with the foreign key.
            modelBuilder.Entity<Parcel>()
                .HasOne(p => p.ResidentUnit)
                .WithMany(ru => ru.Parcels)
                .HasForeignKey(p => p.ResidentUnitId);

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