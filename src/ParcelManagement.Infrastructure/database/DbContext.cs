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
        public DbSet<Parcel> Parcels { get; set; }
        public DbSet<User> Users { get; set; }


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
        }
    }
}