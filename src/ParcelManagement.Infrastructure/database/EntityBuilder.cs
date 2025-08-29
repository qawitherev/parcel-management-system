using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Infrastructure.Database
{
    public class TrackingEventEntityConfiguration : IEntityTypeConfiguration<TrackingEvent>
    {
        public void Configure(EntityTypeBuilder<TrackingEvent> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.TrackingEventType).HasConversion<string>();
            builder.HasOne(te => te.Parcel).WithMany(p => p.TrackingEvents)
                .HasForeignKey(te => te.ParcelId);
        }
    }
}