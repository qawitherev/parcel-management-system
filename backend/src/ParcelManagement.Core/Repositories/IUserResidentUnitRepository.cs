using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface IUserResidentUnitRepository
    {
        Task<UserResidentUnit> CreateUserResidentUnitAsync(UserResidentUnit userResidentUnit);

        Task<UserResidentUnit?> GetUserResidentUnitByIdAsync(Guid id);

        Task UpdateUserResidentUnit(UserResidentUnit userResidentUnit);

        Task UpdateUserResidentUnits(List<UserResidentUnit> userResidentUnits);

        Task DeleteUserResidentUnit(Guid Id);

        Task<IReadOnlyList<UserResidentUnit>> GetUserResidentUnitsBySpecification(ISpecification<UserResidentUnit> specification);

        Task<UserResidentUnit?> GetOneResidentUnitBySpecification(ISpecification<UserResidentUnit> specification);

        // combination means combination of 
        // userId and residentUnitId
        Task<UserResidentUnit?> GetResidentUnitByCombination(Guid userId, Guid residentUnitId);

        Task<IReadOnlyCollection<User?>> GetUsersByResidentUnit(Guid residentUnitId);

        Task<IReadOnlyCollection<ResidentUnit?>> GetResidentUnitsByUser(Guid userId);

        Task<int> GetUserResidentUnitCountBySpecification(ISpecification<UserResidentUnit> specification);

        Task<IReadOnlyList<UserResidentUnit>> AddResidentsForUnitAsync(List<UserResidentUnit> userResidentUnits);
    }
}