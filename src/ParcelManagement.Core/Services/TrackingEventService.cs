using System.Runtime.CompilerServices;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface ITrackingEventService
    {
        Task<(TrackingEvent, Parcel)> ManualEventTracking(string trackingNumber, Guid performedByUser,
            string customEvent
        );
    }

    public class TrackingEventService : ITrackingEventService
    {
        private readonly ITrackingEventRepository _trackingEventRepo;
        private readonly IParcelRepository _parcelRepo;

        public TrackingEventService(ITrackingEventRepository trackingEventRepo,
            IParcelRepository parcelRepo
        )
        {
            _trackingEventRepo = trackingEventRepo;
            _parcelRepo = parcelRepo;
        }

        public async Task<(TrackingEvent, Parcel)> ManualEventTracking(string trackingNumber, Guid performedByUser,
            string customEvent
        )
        {
            var parcel = await _parcelRepo.GetOneParcelBySpecificationAsync(new ParcelByTrackingNumberSpecification(trackingNumber)) ??
                throw new NullReferenceException($"Parcel not found. TrackingNumber: ${trackingNumber}");
            var te = await _trackingEventRepo.CreateAsync(new TrackingEvent
            {
                Id = Guid.NewGuid(),
                ParcelId = parcel.Id,
                TrackingEventType = TrackingEventType.Custom,
                CustomEvent = customEvent,
                EventTime = DateTimeOffset.UtcNow,
                PerformedByUser = performedByUser
            });
            var p = await _parcelRepo.GetParcelByIdAsync(parcel.Id);

            return (te, p!);
        }
    }
}