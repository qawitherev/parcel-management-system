using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface IBaseRepository<T>
    {
        Task<T?> GetOneBySpecificationAsync(ISpecification<T> specification);

        Task<IReadOnlyList<T>> GetBySpecificationAsync(ISpecification<T> specification);

        Task<int> GetCountBySpecificationAsync(ISpecification<T> specification);

        Task<T> CreateAsync(T obj);

        Task<T?> FindByIdAsync(Guid id);
        Task<ICollection<T>>? GetAllAsync();
    }
}