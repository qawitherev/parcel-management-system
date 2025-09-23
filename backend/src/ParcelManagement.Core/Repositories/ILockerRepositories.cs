using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface ILockerRepository
    {
        Task<Locker> CreateLockerAsync(Locker locker);
        Task<Locker?> GetLockerByIdAsync(Guid id);
        Task UpdateLockerAsync(Locker locker);
        Task<IReadOnlyList<Locker>> GetLockersBySpecificationAsync(ISpecification<Locker> specification);
        Task<Locker?> GetOneLockerBySpecification(ISpecification<Locker> specification);
        Task<int> GetLockerCountBySpecification(ISpecification<Locker> specification);
    }
}