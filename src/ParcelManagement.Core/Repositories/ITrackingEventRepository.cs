using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Repositories
{
     public interface ITrackingEventRepository : IBaseRepository<TrackingEvent>
    {
        Task UpdateTrackingEventAsync(TrackingEvent trackingEvent);
    }
}