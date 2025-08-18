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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // configure the column to use string enum instead of int
            modelBuilder.Entity<Parcel>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Parcel>()
                .Property(p => p.Weight)
                .HasPrecision(18, 2);

            // add unique constraint on tracking number and index it
            modelBuilder.Entity<Parcel>()
                .HasIndex(p => p.TrackingNumber)
                .IsUnique();

            // add unique constraint to user.username and index it
            modelBuilder.Entity<User>()
                .HasIndex(user => user.Username)
                .IsUnique();

            // it is a best practice to start from the dependent entity—the one with the foreign key.
            modelBuilder.Entity<Parcel>()
                .HasOne(p => p.ResidentUnit)
                .WithMany(ru => ru.Parcels)
                .HasForeignKey(p => p.ResidentUnitId);
        }
    }
}