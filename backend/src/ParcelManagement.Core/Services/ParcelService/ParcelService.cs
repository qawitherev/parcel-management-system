using ParcelManagement.Core.BackgroundServices;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Model.Parcel;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Core.UnitOfWork;

namespace ParcelManagement.Core.Services
{
    public partial class ParcelService(
        IParcelRepository parcelRepo,
        IResidentUnitRepository residentUnitRepo,
        IUserRepository userRepo,
        ITrackingEventRepository trackingEventRepo,
        ILockerRepository lockerRepo,
        IUnitOfWork unitOfWork, 
        IParcelOverstayEnqueuer parcelOverstayEnqueuer
        ) : IParcelService
    {
        private readonly IParcelRepository _parcelRepo = parcelRepo;
        private readonly IResidentUnitRepository _residentUnitRepo = residentUnitRepo;
        private readonly IUserRepository _userRepo = userRepo;
        private readonly ILockerRepository _lockerRepo = lockerRepo;
        private readonly IUnitOfWork _uow = unitOfWork;
        private readonly IParcelOverstayEnqueuer _parcelOverstayEnqueuer = parcelOverstayEnqueuer;

        private readonly ITrackingEventRepository _trackingEventRepo = trackingEventRepo;
        
        public async Task ClaimParcelAsync(string trackingNumber, Guid performedByUser)
        {
            var spec = new ParcelByTrackingNumberSpecification(trackingNumber);
            var toBeClaimedParcel = await _parcelRepo.GetOneParcelBySpecificationAsync(spec) ??
                throw new InvalidOperationException($"Parcel with tracking number '{trackingNumber}' not found.");
            if (toBeClaimedParcel.Status == ParcelStatus.Claimed)
            {
                throw new InvalidOperationException($"Parcel {trackingNumber} has been claimed!");
            }
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

        public async Task<Parcel> GetParcelByIdAsync(Guid id)
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
            var p = await _parcelRepo.GetOneParcelBySpecificationAsync(new ParcelByTrackingNumberSpecification(trackingNumber)) ??
                throw new KeyNotFoundException($"Parcel {trackingNumber} is not found");
            if (role == UserRole.Resident)
            {
                var parcelByUserSpec = new ParcelByUserSpecification(inquiringUserId);
                var userParcels = await _parcelRepo.GetParcelsBySpecificationAsync(parcelByUserSpec);
                if (!userParcels.Any(up => up?.TrackingNumber == trackingNumber))
                {
                    throw new UnauthorizedAccessException("Parcel does not belong to user");
                }

                if (!userParcels.Any())
                {
                    throw new UnauthorizedAccessException("User has no parcels");
                }
            }
            var spec = new ParcelHistoriesSpecification(p.Id);
            return await _parcelRepo.GetOneParcelBySpecificationAsync(spec) ??
                throw new NullReferenceException("Parcel has no histories");
        }

        public async Task<(IReadOnlyList<Parcel>, int count)> GetParcelsForView(
            UserRole? role, Guid? userId, string? searchKeyword, ParcelStatus? status, string? customEvent, ParcelSortableColumn? column, int? page, int? take = 20,
            bool isAsc = true
            )
        {
            var filterPaginationRequest = new FilterPaginationRequest<ParcelSortableColumn>
            {
                SearchKeyword = searchKeyword,
                Page = page,
                Take = take,
                SortableColumn = column ?? ParcelSortableColumn.TrackingNumber
            };
            var spec = new ParcelViewSpecification(
                filterPaginationRequest,
                role, 
                userId,
                status
            );
            var res = await _parcelRepo.GetParcelsBySpecificationAsync(spec);
            var count = await _parcelRepo.GetParcelCountBySpecification(spec);
            return (res, count);
        }

        public async Task<(IReadOnlyCollection<Parcel>, int count)> GetRecentlyPickedUp()
        {
            var specification = new ParcelRecentlyPickedUpSpecification();
            var parcels = await _parcelRepo.GetParcelsBySpecificationAsync(specification);
            var count = await _parcelRepo.GetParcelCountBySpecification(specification);
            return (parcels, count);
        }

        public async Task<Parcel> CheckInParcelWithLockerAsync(string trackingNumber, string residentUnit, string locker, decimal? weight, string? dimensions, Guid performedByUser)
        {
            // check for trackingNumber, residentUnit, locker legitimacy
            var parcelByTrackingNumberSpec = new ParcelByTrackingNumberSpecification(trackingNumber);
            var existingParcel = await _parcelRepo.GetOneParcelBySpecificationAsync(parcelByTrackingNumberSpec);
            if (existingParcel != null)
            {
                throw new InvalidOperationException($"Parcel {trackingNumber} already checked in");
            }
            var residentByUnitNameSpecification = new ResidentUnitByUnitNameSpecification(residentUnit);
            var existingRu = await _residentUnitRepo.GetOneResidentUnitBySpecificationAsync(residentByUnitNameSpecification) ??
                throw new KeyNotFoundException($"Resident unit {residentUnit} not found");
            var lockerByLockerNameSpecification = new LockerByLockerNameSpecification(locker);
            var existingLocker = await _lockerRepo.GetOneLockerBySpecification(lockerByLockerNameSpecification) ??
                throw new KeyNotFoundException($"Locker {locker} is not found");
            var newParcel = await CheckInHelper(trackingNumber, existingRu.Id, existingLocker.Id, weight, dimensions, performedByUser, 2);
            var parcelWithDetails = await GetParcelDetailsById(newParcel.Id);
            return parcelWithDetails;
        }


        // helpers 
        private async Task<Parcel> CheckInHelper(string trackingNumber, Guid residentUnitId, Guid? lockerId, decimal? weight, string? dimensions, Guid performedByUser, 
            int version = 1, bool isBulk = false
        )
        {
            var newParcel = new Parcel
            {
                Id = Guid.NewGuid(),
                TrackingNumber = trackingNumber,
                ResidentUnitDeprecated = "",
                ResidentUnitId = residentUnitId,
                LockerId = lockerId,
                Status = ParcelStatus.AwaitingPickup,
                Weight = weight ?? 0,
                Dimensions = dimensions ?? "",
                EntryDate = DateTimeOffset.UtcNow, 
                Version = version
            };
            var newTracking = new TrackingEvent
            {
                Id = Guid.NewGuid(),
                ParcelId = newParcel.Id,
                TrackingEventType = isBulk ? TrackingEventType.BulkCheckIn : TrackingEventType.CheckIn,
                EventTime = DateTimeOffset.UtcNow,
                PerformedByUser = performedByUser
            };
            await _parcelRepo.AddParcelAsync(newParcel);
            await _trackingEventRepo.CreateAsync(newTracking);
            return newParcel;
        }

        private async Task<Parcel> GetParcelDetailsById(Guid id)
        {
            var spec = new ParcelDetailsByIdSpecification(id);
            var parcelWithDetails = await _parcelRepo.GetOneParcelBySpecificationAsync(spec) ??
                throw new KeyNotFoundException($"Parcel not found");
            return parcelWithDetails;
        }

        public async Task<BulkClaimResponse> BulkClaimAsync(IEnumerable<string> trackingNumbers, Guid performedByUser)
        {
            var response = new BulkClaimResponse
            {
                InvalidTrackingNumbers = []
            };

            var trackingNumberList = trackingNumbers.ToList();
            if (trackingNumberList.Count == 0)
            {
                response.IsSuccess = false;
                return response;
            }

            // Get all parcels that belong to the user
            var userParcelsSpec = new ParcelByUserSpecification(performedByUser);
            var userParcels = await _parcelRepo.GetParcelsBySpecificationAsync(userParcelsSpec);
            var userParcelsDict = userParcels
                .Where(p => p != null)
                .ToDictionary(p => p!.TrackingNumber, StringComparer.OrdinalIgnoreCase);

            // Validate all tracking numbers first (all-or-nothing strategy)
            var parcelsToClaimList = new List<Parcel>();
            foreach (var trackingNumber in trackingNumberList)
            {
                if (!userParcelsDict.TryGetValue(trackingNumber, out var parcel))
                {
                    response.InvalidTrackingNumbers.Add(trackingNumber);
                    continue;
                }

                if (parcel!.Status == ParcelStatus.Claimed)
                {
                    response.InvalidTrackingNumbers.Add(trackingNumber);
                    continue;
                }

                parcelsToClaimList.Add(parcel);
            }

            // If any tracking number is invalid, return error (all-or-nothing)
            if (response.InvalidTrackingNumbers.Count > 0)
            {
                response.IsSuccess = false;
                return response;
            }

            // All validations passed, proceed with claiming
            foreach (var parcel in parcelsToClaimList)
            {
                parcel.Status = ParcelStatus.Claimed;
                parcel.ExitDate = DateTimeOffset.UtcNow;

                await _parcelRepo.UpdateParcelAsync(parcel);
                await _trackingEventRepo.CreateAsync(new TrackingEvent
                {
                    Id = Guid.NewGuid(),
                    ParcelId = parcel.Id,
                    TrackingEventType = TrackingEventType.BulkClaim,
                    EventTime = DateTimeOffset.UtcNow,
                    PerformedByUser = performedByUser
                });
            }

            response.IsSuccess = true;
            response.ParcelsClaimed = parcelsToClaimList.Count;
            return response;
        }

        public async Task WakeUpProcessParcelOverstay()
        {
            await _parcelOverstayEnqueuer.EnqueueProcessParcelOverstay();
        }
    }
}