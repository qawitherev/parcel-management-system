using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Model.Parcel;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public partial class ParcelService: IParcelService
    {
        public async Task<Parcel> CheckInParcelAsync(string trackingNumber, string residentUnit,
            decimal? weight,
            string? dimensions,
            Guid performedByUser
            )
        {
            // check if residentUnit exist 
            var specByUnitName = new ResidentUnitByUnitNameSpecification(residentUnit);
            var existingRu = await _residentUnitRepo.GetOneResidentUnitBySpecificationAsync(specByUnitName) ??
                throw new NullReferenceException($"Resident unit {residentUnit} not found");

            //check for parcel with the same tracking number 
            var spec = new ParcelByTrackingNumberSpecification(trackingNumber);
            var sameParcel = await _parcelRepo.GetOneParcelBySpecificationAsync(spec);
            if (sameParcel != null)
            {
                throw new InvalidOperationException($"A parcel with tracking number '{trackingNumber}' already exists.");
            }
            var newParcel = await CheckInHelper(trackingNumber, existingRu.Id, null, weight, dimensions, performedByUser);
            return newParcel;
        }

          public async Task<BulkCheckInResponse> BulkCheckInAsync(
            IEnumerable<(string trackingNumber, string residentUnit, string? lockerName, decimal? weight, string? dimensions)> inParcels,
            Guid performedByUser
        )
        {
            var response = new BulkCheckInResponse()
            {
                Items = []
            };

            var existingResidentUnit = await _residentUnitRepo.GetResidentUnitsAsync();
            var existingResidentUnitDict = existingResidentUnit.ToDictionary(
                ru => ru.UnitName,
                StringComparer.OrdinalIgnoreCase
            );

            var existingParcels = await _parcelRepo.GetAllParcelsAsync();
            var existingParcelsDict = existingParcels.ToDictionary(
                p => p!.TrackingNumber,
                StringComparer.OrdinalIgnoreCase
            );

            // a check to determine if we're passing locker or not 
            // all or nothing check 
            if (!((inParcels.Count() == inParcels.Count(ip => ip.lockerName == null)) || 
                (inParcels.Count() == inParcels.Count(ip => ip.lockerName != null)))
                )
            {
                throw new InvalidOperationException($"Locker name for all rows must be provided");
            }
            var isV2 = inParcels.Any(ip => ip.lockerName != null);

            Dictionary<string, Locker>? existingLockerDict = null;
            if (isV2)
            {
                var allLockerSpec = new AllLockersSpecification(new FilterPaginationRequest<LockerSortableColumn> { });
                var existingLockers = await _lockerRepo.GetLockersBySpecificationAsync(allLockerSpec);
                existingLockerDict = existingLockers.ToDictionary(locker => locker.LockerName, StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                int currentRow = 0;
                var isError = false;
                var isRowError = false;
                foreach (var (trackingNumber, residentUnit, locker, weight, dimensions) in inParcels)
                {
                    currentRow++;
                    isRowError = false;
                    if (!existingResidentUnitDict.TryGetValue(residentUnit, out var ru))
                    {
                        response.Items.Add(new ParcelCheckInResponse
                        {
                            TrackingNumber = trackingNumber,
                            Row = currentRow,
                            IsError = true,
                            Message = $"Resident unit {residentUnit} not found"
                        });
                        isError = true;
                        isRowError = true;
                        // throw new Exception($"Resident unit {residentUnit} not found");
                    }
                    if (!existingLockerDict!.TryGetValue(locker!, out var theLocker) && isV2)
                    {
                        response.Items.Add(new ParcelCheckInResponse
                        {
                            TrackingNumber = trackingNumber,
                            Row = currentRow,
                            IsError = true,
                            Message = $"Locker {locker} not found"
                        });
                        isError = true;
                        isRowError = true;    
                    }
                    if (existingParcelsDict.TryGetValue(trackingNumber, out var parcel))
                    {
                        response.Items.Add(new ParcelCheckInResponse
                        {
                            TrackingNumber = trackingNumber,
                            Row = currentRow,
                            IsError = true,
                            Message = $"Parcel {trackingNumber} already checked in"
                        });
                        isError = true;
                        isRowError = true;
                        // throw new Exception($"Parcel {trackingNumber} already checked in");
                    }
                    if (!isRowError)
                    {
                        response.Items.Add(new ParcelCheckInResponse
                        {
                            TrackingNumber = trackingNumber,
                            Row = currentRow,
                            IsError = false,
                        });
                    }
                    if (isError)
                    {
                        continue;
                    }
                    var newParcel = await CheckInHelper(
                        trackingNumber, existingResidentUnitDict[residentUnit].Id, existingLockerDict[locker!].Id, weight, dimensions, performedByUser,
                        isV2 ? 2 : 1, isBulk: true);
                    existingParcelsDict[trackingNumber] = newParcel;
                }
                if (response.Items.Any(i => i.IsError))
                {
                    throw new Exception();
                }
            }
            catch
            {
                return response;
            }
            return response;
        }
    }

          
}