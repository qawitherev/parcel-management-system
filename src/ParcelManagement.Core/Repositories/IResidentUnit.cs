using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface IResidentUnitRepository
    {
        Task<ResidentUnit> CreateResidentUnit(ResidentUnit residentUnit);

        Task<ResidentUnit?> GetResidentUnitById(Guid id);

        Task<IReadOnlyList<ResidentUnit?>> GetResidentUnits();

        Task UpdateResidenUnit(ResidentUnit residentUnit);

        Task DeleteResidentUnit(Guid id);

        // my conscience state that here we better to use 
        // IReadOnlyCollection<T> because of many functionality e.g., 
        // indexer i.e., res[0] --> to access first element 
        Task<IReadOnlyList<ResidentUnit?>> GetResidentUnitsBySpecification(ISpecification<ResidentUnit> specification);

        Task<ResidentUnit?> GetOneResidentUnitBySpecification(ISpecification<ResidentUnit> specification);
    }
}