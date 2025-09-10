using ParcelManagement.Core.Entities;
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

        public async Task<IReadOnlyCollection<User?>> GetUsersByResidentUnit(Guid residentUnitId)
        {
            return await _uruRepo.GetUsersByResidentUnit(residentUnitId);
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