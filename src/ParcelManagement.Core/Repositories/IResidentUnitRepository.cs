using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface IResidentUnitRepository
    {
        Task<ResidentUnit> CreateResidentUnitAsync(ResidentUnit residentUnit);

        Task<ResidentUnit?> GetResidentUnitByIdAsync(Guid id);

        Task<IReadOnlyList<ResidentUnit?>> GetResidentUnitsAsync();

        Task UpdateResidenUnitAsync(ResidentUnit residentUnit);

        Task DeleteResidentUnitAsync(Guid id);

        // my conscience state that here we better to use 
        // IReadOnlyCollection<T> because of many functionality e.g., 
        // indexer i.e., res[0] --> to access first element 
        Task<IReadOnlyList<ResidentUnit?>> GetResidentUnitsBySpecificationAsync(ISpecification<ResidentUnit> specification);

        Task<ResidentUnit?> GetOneResidentUnitBySpecificationAsync(ISpecification<ResidentUnit> specification);
    }
}