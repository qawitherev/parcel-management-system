using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class TrackingEventRepository : BaseRepository<TrackingEvent>, ITrackingEventRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public TrackingEventRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateTrackingEventAsync(TrackingEvent trackingEvent)
        {
            var existingEvent = await _dbContext.TrackingEvents.FindAsync(trackingEvent.Id) ??
                throw new KeyNotFoundException($"Tracking event not found");
            _dbContext.Entry(existingEvent).CurrentValues.SetValues(trackingEvent);
            await _dbContext.SaveChangesAsync();
        }
    }
}