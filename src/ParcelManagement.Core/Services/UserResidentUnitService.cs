using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IUserResidentUnit
    {
        Task<UserResidentUnit> CreateUserResidentUnit(UserResidentUnit userResidentUnit, Guid creator);

        Task UpdateUserResidentUnit(UserResidentUnit userResidentUnit);

        Task<IReadOnlyCollection<User?>> GetUsersByResidentUnit(Guid residentUnitId);

        Task<IReadOnlyCollection<ResidentUnit?>> GetResidentsUnitByUser(Guid userId);
    }

    public class UserResidentUnitService(IUserResidentUnitRepository repository) : IUserResidentUnit
    {
        private readonly IUserResidentUnitRepository _repository = repository;
        public async Task<UserResidentUnit> CreateUserResidentUnit(UserResidentUnit userResidentUnit, Guid creator)
        {
            var res = await _repository.GetResidentUnitByCombination(
                userResidentUnit.UserId,
                userResidentUnit.ResidentUnitId);
            if (res == null)
            {
                await _repository.CreateUserResidentUnitAsync(userResidentUnit);
                return userResidentUnit;
            }
            else if (res != null && !res.IsActive)
            {
                userResidentUnit.IsActive = true;
                await _repository.UpdateUserResidentUnit(userResidentUnit);
                return userResidentUnit;
            }
            else
            {
                throw new InvalidOperationException($"Entry already exist in UserResidentUnit");
            }
        }

        public async Task<IReadOnlyCollection<ResidentUnit?>> GetResidentsUnitByUser(Guid userId)
        {
            return await _repository.GetResidentUnitsByUser(userId);
        }

        public async Task<IReadOnlyCollection<User?>> GetUsersByResidentUnit(Guid residentUnitId)
        {
            return await _repository.GetUsersByResidentUnit(residentUnitId);
        }

        public async Task UpdateUserResidentUnit(UserResidentUnit userResidentUnit)
        {
            var existing = await _repository.GetResidentUnitByCombination(
                userResidentUnit.UserId,
                userResidentUnit.ResidentUnitId
            ) ?? throw new NullReferenceException($"Entry UserResidentUnit does not exist");
            existing.IsActive = userResidentUnit.IsActive;
            await _repository.UpdateUserResidentUnit(userResidentUnit);
        }
    }
}