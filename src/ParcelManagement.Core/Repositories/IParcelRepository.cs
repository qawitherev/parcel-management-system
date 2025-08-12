using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface IParcelRepository
    {
        Task<Parcel?> GetParcelByIdAsync(Guid id);
        Task<IReadOnlyList<Parcel?>> GetAllParcelsAsync();
        Task<Parcel> AddParcelAsync(Parcel parcel);
        Task UpdateParcelAsync(Parcel parcel);
        Task DeleteParcelAsync(Guid id);
        Task<IReadOnlyList<Parcel?>> GetParcelsBySpecificationAsync(ISpecification<Parcel> specification);
        Task<Parcel?> GetOneParcelBySpecificationAsync(ISpecification<Parcel> specification);
    }
}