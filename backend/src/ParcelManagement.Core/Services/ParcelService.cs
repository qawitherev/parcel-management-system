using Microsoft.AspNetCore.Authorization;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IParcelService
    {
        // check in, claim, getByTrackingNumber, getAll (to be implemented later: getByResidentUnit)
        Task<Parcel> CheckInParcelAsync(string trackingNumber, string residentUnit,
            decimal? weight,
            string? dimensions, Guid performedByUser);

        Task ClaimParcelAsync(string trackingNumber, Guid performedByUser);

        Task<Parcel?> GetParcelByIdAsync(Guid id);

        Task<IReadOnlyList<Parcel?>> GetAllParcelAsync();

        Task<(IReadOnlyList<Parcel?> Parcels, int Count)> GetAwaitingPickupParcelsAsync();

        Task<Parcel?> GetParcelByTrackingNumberAsync(string trackingNumber);

        Task<IReadOnlyList<Parcel?>> GetParcelByResidentUnitAsync(string residentUnit);

        Task<(IReadOnlyList<Parcel?>, int count)> GetParcelByUser(Guid userId, ParcelStatus? status = null);

        Task<Parcel> GetParcelHistoriesAsync(string trackingNumber, Guid inquiringUserId, UserRole role);

        Task<(IReadOnlyCollection<Parcel>, int count)> GetRecentlyPickedUp(); 
    }

    public class ParcelService(
        IParcelRepository parcelRepo,
        IResidentUnitRepository residentUnitRepo,
        IUserRepository userRepo,
        ITrackingEventRepository trackingEventRepo
        ) : IParcelService
    {
        private readonly IParcelRepository _parcelRepo = parcelRepo;
        private readonly IResidentUnitRepository _residentUnitRepo = residentUnitRepo;
        private readonly IUserRepository _userRepo = userRepo;

        private readonly ITrackingEventRepository _trackingEventRepo = trackingEventRepo;

        public async Task<Parcel> CheckInParcelAsync(string trackingNumber, string residentUnit,
            decimal? weight,
            string? dimensions,
            Guid performedByUser
            )
        {
            // check if residentUnit exist 
            var specByUnitName = new ResidentUnitByUnitNameSpecification(residentUnit);
            var realResidentUnit = await _residentUnitRepo.GetOneResidentUnitBySpecificationAsync(specByUnitName) ??
                throw new NullReferenceException($"Resident unit {residentUnit} not found");

            //check for parcel with the same tracking number 
            var spec = new ParcelByTrackingNumberSpecification(trackingNumber);
            var sameParcel = await _parcelRepo.GetOneParcelBySpecificationAsync(spec);
            if (sameParcel != null)
            {
                throw new InvalidOperationException($"A parcel with tracking number '{trackingNumber}' already exists.");
            }
            var newParcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = trackingNumber,
                ResidentUnitDeprecated = residentUnit,
                ResidentUnitId = realResidentUnit.Id,
                Status = ParcelStatus.AwaitingPickup,
                Weight = weight ?? 0,
                Dimensions = dimensions ?? "", 
                EntryDate = DateTimeOffset.UtcNow
            };

            await _parcelRepo.AddParcelAsync(newParcel);
            await _trackingEventRepo.CreateAsync(new TrackingEvent
            {
                Id = Guid.NewGuid(),
                ParcelId = newParcel.Id,
                TrackingEventType = TrackingEventType.CheckIn,
                EventTime = DateTimeOffset.UtcNow,
                PerformedByUser = performedByUser
            });
            return newParcel;
        }

        public async Task ClaimParcelAsync(string trackingNumber, Guid performedByUser)
        {
            var spec = new ParcelByTrackingNumberSpecification(trackingNumber);
            var toBeClaimedParcel = await _parcelRepo.GetOneParcelBySpecificationAsync(spec) ??
                throw new InvalidOperationException($"Parcel with tracking number '{trackingNumber}' not found.");
            toBeClaimedParcel.Status = ParcelStatus.Claimed;
            toBeClaimedParcel.ExitDate = DateTime.UtcNow;

            await _parcelRepo.UpdateParcelAsync(toBeClaimedParcel);
            await _trackingEventRepo.CreateAsync(new TrackingEvent
            {
                Id = Guid.NewGuid(),
                ParcelId = toBeClaimedParcel.Id,
                TrackingEventType = TrackingEventType.Claim,
                EventTime = DateTimeOffset.UtcNow,
                PerformedByUser = performedByUser
            });
        }

        // why we use nullable here is because this isnt the place to handle it
        // we'll handle it inside controller, to return 404 🫡
        public async Task<IReadOnlyList<Parcel?>> GetAllParcelAsync()
        {
            return await _parcelRepo.GetAllParcelsAsync();
        }

        public async Task<(IReadOnlyList<Parcel?> Parcels, int Count)> GetAwaitingPickupParcelsAsync()
        {
            var specification = new ParcelsAwaitingPickupSpecification();
            var parcels = await _parcelRepo.GetParcelsBySpecificationAsync(specification);
            var count = await _parcelRepo.GetParcelCountBySpecification(specification);
            return (parcels, count);
        }

        public async Task<Parcel?> GetParcelByIdAsync(Guid id)
        {
            var parcel = await _parcelRepo.GetParcelByIdAsync(id) ?? throw new KeyNotFoundException($"Parcel with id {id} is not found");
            return parcel;
        }

        public async Task<IReadOnlyList<Parcel?>> GetParcelByResidentUnitAsync(string residentUnit)
        {
            var parcelByResidentUnitSpec = new ParcelsByResidentUnitSpecification(residentUnit);
            return await _parcelRepo.GetParcelsBySpecificationAsync(parcelByResidentUnitSpec);
        }

        public async Task<Parcel?> GetParcelByTrackingNumberAsync(string trackingNumber)
        {
            var specification = new ParcelByTrackingNumberSpecification(trackingNumber);
            var parcel = await _parcelRepo.GetOneParcelBySpecificationAsync(specification) ?? throw new KeyNotFoundException($"Parcel with tracking number {trackingNumber} not found");
            return parcel;
        }

        public async Task<(IReadOnlyList<Parcel?>, int count)> GetParcelByUser(Guid userId, ParcelStatus? status = null)
        {
            var user = await _userRepo.GetUserByIdAsync(userId) ??
                throw new KeyNotFoundException($"User not found");
            var spec = new ParcelByUserSpecification(userId, status);
            var parcels = await _parcelRepo.GetParcelsBySpecificationAsync(spec);
            var count = await _parcelRepo.GetParcelCountBySpecification(spec);
            return (parcels, count);
        }

        public async Task<Parcel> GetParcelHistoriesAsync(string trackingNumber, Guid inquiringUserId, UserRole role)
        {
            // check if the parcel belongs to the accessing user 
            var user = await _userRepo.GetUserByIdAsync(inquiringUserId) ??
                throw new KeyNotFoundException("User not found");
            var parcelByUserSpec = new ParcelByUserSpecification(inquiringUserId);
            var userParcels = await _parcelRepo.GetParcelsBySpecificationAsync(parcelByUserSpec);
            if (!userParcels.Any())
            {
                throw new UnauthorizedAccessException("User has no parcels");
            }
            if (!userParcels.Any(up => up?.TrackingNumber == trackingNumber))
            {
                throw new UnauthorizedAccessException("Parcel does not belong to user");
            }
            var p = await _parcelRepo.GetOneParcelBySpecificationAsync(new ParcelByTrackingNumberSpecification(trackingNumber)) ??
                throw new KeyNotFoundException($"Parcel {trackingNumber} is not found");
            var spec = new ParcelHistoriesSpecification(p.Id);
            return await _parcelRepo.GetOneParcelBySpecificationAsync(spec) ??
                throw new NullReferenceException("Parcel has no histories");
        }

        public async Task<(IReadOnlyCollection<Parcel>, int count)> GetRecentlyPickedUp()
        {
            var specification = new ParcelRecentlyPickedUpSpecification();
            var parcels = await _parcelRepo.GetParcelsBySpecificationAsync(specification);
            var count = await _parcelRepo.GetParcelCountBySpecification(specification);
            return (parcels, count);
        }
    }
}