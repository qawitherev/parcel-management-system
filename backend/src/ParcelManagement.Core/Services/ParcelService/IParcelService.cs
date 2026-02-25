using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Parcel;

namespace ParcelManagement.Core.Services
{
        public interface IParcelService
    {
        Task<Parcel> GetParcelByIdAsync(Guid id);

        // check in, claim, getByTrackingNumber, getAll (to be implemented later: getByResidentUnit)
        Task<Parcel> CheckInParcelAsync(string trackingNumber, string residentUnit,
            decimal? weight,
            string? dimensions, Guid performedByUser);

        Task<Parcel> CheckInParcelWithLockerAsync(
            string trackingNumber, string residentUnit,
            string locker,
            decimal? weight,
            string? dimensions, Guid performedByUser
        );

        Task<BulkCheckInResponse> BulkCheckInAsync(
            IEnumerable<(string trackingNumber, string residentUnit, string? lockerName, decimal? weight, string? dimensions)> parcels,
            Guid performedByUser);

        Task<(IReadOnlyList<Parcel>, int count)> GetParcelsForView(
            UserRole? role,
            Guid? userId,
            string? trackingNumber,
            ParcelStatus? status,
            string? customEvent,
            ParcelSortableColumn? column,
            int? page, int? take = 20, 
            bool isAsc = true
        );


        Task ClaimParcelAsync(string trackingNumber, Guid performedByUser);

        Task<IReadOnlyList<Parcel?>> GetAllParcelAsync();

        Task<(IReadOnlyList<Parcel?> Parcels, int Count)> GetAwaitingPickupParcelsAsync();

        Task<Parcel?> GetParcelByTrackingNumberAsync(string trackingNumber);

        Task<IReadOnlyList<Parcel?>> GetParcelByResidentUnitAsync(string residentUnit);

        Task<(IReadOnlyList<Parcel?>, int count)> GetParcelByUser(Guid userId, ParcelStatus? status = null);

        Task<Parcel> GetParcelHistoriesAsync(string trackingNumber, Guid inquiringUserId, UserRole role);

        Task<(IReadOnlyCollection<Parcel>, int count)> GetRecentlyPickedUp();

        Task<BulkClaimResponse> BulkClaimAsync(IEnumerable<string> trackingNumbers, Guid performedByUser);

        Task<int> UpdateOverstayedParcel(); 

        Task WakeUpProcessParcelOverstay();
    }

}