using ParcelManagement.Core.Specifications;
using Microsoft.EntityFrameworkCore.Storage;

namespace ParcelManagement.Core.Repositories
{
    public interface IBaseRepository<T>
    {
        Task<T?> GetOneBySpecificationAsync(ISpecification<T> specification);

        Task<IReadOnlyList<T>> GetBySpecificationAsync(ISpecification<T> specification);

        Task<int> GetCountBySpecificationAsync(ISpecification<T> specification);

        Task<T> CreateAsync(T obj);

        Task<IReadOnlyList<T>> CreateRangeAsync(List<T> objs);

        Task<T?> GetByIdAsync(Guid id);

        Task<ICollection<T>>? GetAllAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();

        Task<int> DeleteAsync(Guid id);

        Task<int> DeleteRangeAsync(IEnumerable<Guid> ids);
    }
}