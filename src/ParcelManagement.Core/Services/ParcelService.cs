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
            string? dimensions);

        Task ClaimParcelAsync(string trackingNumber);

        Task<Parcel?> GetParcelByIdAsync(Guid id);

        Task<IReadOnlyList<Parcel?>> GetAllParcelAsync();

        Task<IReadOnlyList<Parcel?>> GetAwaitingPickupParcelsAsync();

        Task<Parcel?> GetParcelByTrackingNumberAsync(string trackingNumber);

        Task<IReadOnlyList<Parcel?>> GetParcelByResidentUnitAsync(string residentUnit);

        Task<IReadOnlyList<Parcel?>> GetParcelsAwaitingPickup();
        
    }

    public class ParcelService : IParcelService
    {
        private readonly IParcelRepository _parcelRepo;

        public ParcelService(IParcelRepository parcelRepo)
        {
            _parcelRepo = parcelRepo;
        }

        public async Task<Parcel> CheckInParcelAsync(string trackingNumber, string residentUnit,
            decimal? weight,
            string? dimensions)
        {
            var newParcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = trackingNumber,
                ResidentUnit = residentUnit,
                Status = ParcelStatus.AwaitingPickup,
                Weight = weight ?? 0,
                Dimensions = dimensions ?? ""
            };

            //check for parcel with the same tracking number 
            var spec = new ParcelByTrackingNumberSpecification(newParcel.TrackingNumber);
            var sameParcel = await _parcelRepo.FindOneBySpecificationAsync(spec);
            if (sameParcel != null)
            {
                throw new InvalidOperationException($"A parcel with tracking number '{trackingNumber}' already exists.");
            }
            return await _parcelRepo.AddParcelAsync(newParcel);
        }

        public async Task ClaimParcelAsync(string trackingNumber)
        {
            var spec = new ParcelByTrackingNumberSpecification(trackingNumber);
            var toBeClaimedParcel = await _parcelRepo.FindOneBySpecificationAsync(spec) ??
                throw new InvalidOperationException($"Parcel with tracking number '{trackingNumber}' not found.");
            toBeClaimedParcel.Status = ParcelStatus.Claimed;
            toBeClaimedParcel.ExitDate = DateTime.UtcNow;
            await _parcelRepo.UpdateParcelAsync(toBeClaimedParcel);
        }

        // why we use nullable here is because this isnt the place to handle it
        // we'll handle it inside controller, to return 404 🫡
        public async Task<IReadOnlyList<Parcel?>> GetAllParcelAsync()
        {
            return await _parcelRepo.GetAllParcelsAsync();
        }

        public async Task<IReadOnlyList<Parcel?>> GetAwaitingPickupParcelsAsync()
        {
            var specification = new ParcelsAwaitingPickupSpecification();
            return await _parcelRepo.FindBySpecificationAsync(specification);
        }

        public async Task<Parcel?> GetParcelByIdAsync(Guid id)
        {
            var parcel = await _parcelRepo.GetParcelByIdAsync(id) ?? throw new KeyNotFoundException($"Parcel with id {id} is not found");
            return parcel;
        }

        public async Task<IReadOnlyList<Parcel?>> GetParcelByResidentUnitAsync(string residentUnit)
        {
            var parcelByResidentUnitSpec = new ParcelsByResidentUnitSpecification(residentUnit);
            return await _parcelRepo.FindBySpecificationAsync(parcelByResidentUnitSpec);
        }

        public async Task<Parcel?> GetParcelByTrackingNumberAsync(string trackingNumber)
        {
            var specification = new ParcelByTrackingNumberSpecification(trackingNumber);
            var parcel = await _parcelRepo.FindOneBySpecificationAsync(specification) ?? throw new KeyNotFoundException($"Parcel with tracking number {trackingNumber} not found");
            return parcel;
        }

        public async Task<IReadOnlyList<Parcel?>> GetParcelsAwaitingPickup()
        {
            var spec = new ParcelsAwaitingPickupSpecification();
            return await _parcelRepo.FindBySpecificationAsync(spec);
        }
    }
}