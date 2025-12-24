using System.Data.Common;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Misc;

namespace ParcelManagement.Infrastructure.Database
{
    public class ParcelEntityConfiguration : IEntityTypeConfiguration<Parcel>
    {
        public void Configure(EntityTypeBuilder<Parcel> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Status).HasConversion<string>();
            builder.Property(p => p.Weight).HasPrecision(18, 2);
            builder.HasIndex(p => p.TrackingNumber).IsUnique();
            builder.HasOne(p => p.ResidentUnit).WithMany(ru => ru.Parcels).HasForeignKey(p => p.ResidentUnitId);
            builder.HasOne(p => p.Locker).WithMany(locker => locker.Parcels).HasForeignKey(p => p.LockerId)
                .IsRequired(false);
            builder.ToTable(t => t.HasCheckConstraint("CK_Parcels_LockerRequired",
            "(`Version` = 1) OR (`Version` > 1 AND `LockerId` IS NOT NULL)"
            ));

        }
    }

    public class TrackingEventEntityConfiguration : IEntityTypeConfiguration<TrackingEvent>
    {
        public void Configure(EntityTypeBuilder<TrackingEvent> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.TrackingEventType).HasConversion<string>();
            builder.HasOne(te => te.Parcel).WithMany(p => p.TrackingEvents)
                .HasForeignKey(te => te.ParcelId);
            builder.HasOne(te => te.User).WithMany().HasForeignKey(te => te.PerformedByUser);
        }
    }

    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Role).HasConversion<string>();
            builder.HasIndex(u => u.Username).IsUnique();
        }
    }

    public class ResidentUnitEntityConfiguration : IEntityTypeConfiguration<ResidentUnit>
    {
        public void Configure(EntityTypeBuilder<ResidentUnit> builder)
        {
            builder.HasKey(ru => ru.Id);
            builder.HasIndex(ru => ru.UnitName).IsUnique();
            builder.HasOne(ru => ru.CreatedByUser).WithMany().HasForeignKey(ru => ru.CreatedBy);
            builder.HasOne(ru => ru.UpdatedByUser).WithMany().HasForeignKey(ru => ru.UpdatedBy)
                .IsRequired(false);

        }
    }

    public class LockerEntityConfiguration : IEntityTypeConfiguration<Locker>
    {
        public void Configure(EntityTypeBuilder<Locker> builder)
        {
            builder.HasKey(l => l.Id);
            builder.HasIndex(l => l.LockerName).IsUnique();
            builder.HasOne(l => l.CreatedByUser).WithMany().HasForeignKey(l => l.CreatedBy);
            builder.HasOne(l => l.UpdatedByUser).WithMany().HasForeignKey(l => l.UpdatedBy).IsRequired(false);
        }
    }

    public class NotificationPrefEntityConfiguration : IEntityTypeConfiguration<NotificationPref>
    {
        public void Configure(EntityTypeBuilder<NotificationPref> builder)
        {
            builder.HasKey(np => np.Id);
            builder.HasOne(np => np.User)
                .WithOne(u => u.NotificationPref)
                .HasForeignKey<NotificationPref>(np => np.UserId)
                .IsRequired();
            builder.HasOne(np => np.CreatingUser)
                .WithMany()
                .HasForeignKey(np => np.CreatedBy);
            builder.HasOne(np => np.UpdatingUser)
                .WithMany()
                .HasForeignKey<NotificationPref>(np => np.UpdatedBy);
        }
    }
}