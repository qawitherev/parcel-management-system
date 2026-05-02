using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Core.UnitOfWork;

namespace ParcelManagement.Core.Services
{
    public partial class ParcelService: IParcelService
    {
        const int OVERSTAY_THRESHOLD_DAYS = 7;
         async Task<int> IParcelService.UpdateOverstayedParcel()
        {
            var specification = new ParcelOverstaySpecification(OVERSTAY_THRESHOLD_DAYS);
            var overstayedParcels = await _parcelRepo.GetParcelsBySpecificationAsync(specification);
            foreach (var parcel in overstayedParcels)
            {
                parcel.OverstayDays += 1;
                parcel.Status = parcel.OverstayDays > 30 ? ParcelStatus.Unclaimed
                    : ParcelStatus.Overstayed;
            }
            await _uow.SaveChangesAsync();
            return overstayedParcels.Count;
        }
    }
}