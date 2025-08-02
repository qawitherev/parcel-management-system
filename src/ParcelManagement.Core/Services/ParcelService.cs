using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IParcelService
    {
        // check in, claim, getByTrackingNumber, getAll (to be implemented later: getByResidentUnit)
        Task<Parcel> CheckInParcelAsync(Parcel parcel);

        Task ClaimParcelAsync(Parcel parcel);

        Task<Parcel?> GetParcelByIdAsync(Guid id);

        Task<IReadOnlyList<Parcel?>> GetAllParcelAsync();

        Task<IReadOnlyList<Parcel?>> GetAwaitingPickupParcelsAsync();

        Task<Parcel?> GetParcelByTrackingNumberAsync(string trackingNumber);
        
    }

    public class ParcelService : IParcelService
    {
        private readonly IParcelRepository _parcelRepo;

        public ParcelService(IParcelRepository parcelRepo)
        {
            _parcelRepo = parcelRepo;
        }

        public async Task<Parcel> CheckInParcelAsync(Parcel newParcel)
        {
            newParcel.Id = Guid.NewGuid();
            newParcel.EntryDate = DateTimeOffset.UtcNow;
            newParcel.Status = ParcelStatus.AwaitingPickup;
            return await _parcelRepo.AddParcelAsync(newParcel);
        }

        public async Task ClaimParcelAsync(Parcel parcel)
        {
            parcel.ExitDate = DateTimeOffset.UtcNow;
            parcel.Status = ParcelStatus.Claimed;
            await _parcelRepo.UpdateParcelAsync(parcel);
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

            return await _parcelRepo.GetParcelByIdAsync(id);
        }

        public async Task<Parcel?> GetParcelByTrackingNumberAsync(string trackingNumber)
        {
            var specification = new ParcelByTrackingNumberSpecification(trackingNumber);
            return await _parcelRepo.FindOneBySpecificationAsync(specification);
        }
    }
}