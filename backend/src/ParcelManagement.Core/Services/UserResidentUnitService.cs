using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IUserResidentUnitService
    {
        Task<UserResidentUnit> CreateUserResidentUnit(Guid creator,
            Guid userId, Guid residentUnitId
        );

        Task UpdateUserResidentUnit(UserResidentUnit userResidentUnit);

        Task<IReadOnlyCollection<User?>> GetUsersByResidentUnit(Guid residentUnitId);

        Task<IReadOnlyCollection<ResidentUnit?>> GetResidentsUnitByUser(Guid userId);

        Task<(IReadOnlyList<UserResidentUnit>, int count)> GetUserResidentUnitForView(
            string? searchKeyword,
            UserResidentUnitSortableColumn? column,
            int? page, int? take = 20,
            bool isAsc = true
        );

        Task UpdateUnitResidents(List<Guid> newResidents, Guid residentUnitId, Guid createdBy);
    }

    public class UserResidentUnitService(
        IUserResidentUnitRepository uruRepo,
        IResidentUnitRepository ruRepo,
        IUserRepository userRepo
        ) : IUserResidentUnitService
    {
        private readonly IUserResidentUnitRepository _uruRepo = uruRepo;
        private readonly IResidentUnitRepository _ruRepo = ruRepo;
        private readonly IUserRepository _userRepo = userRepo;
        
        public async Task<UserResidentUnit> CreateUserResidentUnit(Guid creator,
            Guid userId, Guid residentUnitId
        )
        {
            var user = await _userRepo.GetUserByIdAsync(userId) ?? throw new NullReferenceException($"User ${userId} not found");
            var ru = await _ruRepo.GetResidentUnitByIdAsync(residentUnitId) ?? throw new NullReferenceException($"Resident unit not found");
            var res = await _uruRepo.GetResidentUnitByCombination(
                userId,
                residentUnitId);
            if (res == null)
            {
                return await _uruRepo.CreateUserResidentUnitAsync(new UserResidentUnit
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ResidentUnitId = residentUnitId,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow, 
                    CreatedBy = creator
                });
                
            }
            else if (res != null && !res.IsActive)
            {
                res.IsActive = true;
                await _uruRepo.UpdateUserResidentUnit(res);
                return res;
            }
            else
            {
                throw new InvalidOperationException($"Entry already exist in UserResidentUnit");
            }
        }

        public async Task<IReadOnlyCollection<ResidentUnit?>> GetResidentsUnitByUser(Guid userId)
        {
            return await _uruRepo.GetResidentUnitsByUser(userId);
        }

        public async Task<(IReadOnlyList<UserResidentUnit>, int count)> GetUserResidentUnitForView(string? searchKeyword, UserResidentUnitSortableColumn? column, int? page, int? take = 20, bool isAsc = true)
        {
            var filterPaginationRequest = new FilterPaginationRequest<UserResidentUnitSortableColumn>
            {
                SearchKeyword = searchKeyword,
                Page = page,
                Take = take,
                SortableColumn = column ?? UserResidentUnitSortableColumn.ResidentUnit,
                IsAscending = isAsc
            };
            var viewSpecification = new UserResidentUnitUnitViewSpecification(filterPaginationRequest);
            var userResidentUnit = await _uruRepo.GetUserResidentUnitsBySpecification(viewSpecification);
            var count = await _uruRepo.GetUserResidentUnitCountBySpecification(viewSpecification);
            return (userResidentUnit, count);
        }



        public async Task<IReadOnlyCollection<User?>> GetUsersByResidentUnit(Guid residentUnitId)
        {
            return await _uruRepo.GetUsersByResidentUnit(residentUnitId);
        }

        public async Task UpdateUnitResidents(List<Guid> newResidents, Guid residentUnitId, Guid createdBy)
        {
            var specification = new ResidentUnitResidentsSpecification(residentUnitId);
            var oldResidents = await _ruRepo.GetOneResidentUnitBySpecificationAsync(specification) ??
                throw new KeyNotFoundException("Resident unit does not exist");
            oldResidents.UserResidentUnits = [.. oldResidents.UserResidentUnits.Where(uru => uru.IsActive)];
            var invalidUserIds = await _userRepo.GetInvalidUserId(newResidents);
            if (invalidUserIds.Count > 0)
            {
                var appended = string.Join(", ", invalidUserIds);
                throw new KeyNotFoundException($"Invalid user ids: ${appended}");
            }
            var toRemove = oldResidents.UserResidentUnits.Where(uru => !newResidents.Any(newResident => newResident == uru.UserId));
            var toAdd = newResidents.Where(newResident => !oldResidents.UserResidentUnits.Any(uru => uru.UserId == newResident));
            var toAddResidents = toAdd.Select(ta => new UserResidentUnit
            {
                Id = Guid.NewGuid(),
                UserId = ta,
                ResidentUnitId = residentUnitId,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdBy
            }).ToList();
            await _uruRepo.AddResidentsForUnitAsync(toAddResidents);
            var toRemoveResidents = toRemove.ToList();
            toRemoveResidents.ForEach(trr => trr.IsActive = false);
            await _uruRepo.UpdateUserResidentUnits(toRemoveResidents);
        }

        public async Task UpdateUserResidentUnit(UserResidentUnit userResidentUnit)
        {
            var existing = await _uruRepo.GetResidentUnitByCombination(
                userResidentUnit.UserId,
                userResidentUnit.ResidentUnitId
            ) ?? throw new NullReferenceException($"Entry UserResidentUnit does not exist");
            existing.IsActive = userResidentUnit.IsActive;
            await _uruRepo.UpdateUserResidentUnit(userResidentUnit);
        }
    }
}