using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class LockerRepository : BaseRepository<Locker>, ILockerRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public LockerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Locker> CreateLockerAsync(Locker locker)
        {
            await _dbContext.Lockers.AddAsync(locker);
            await _dbContext.SaveChangesAsync();
            return locker;
        }

        public async Task<Locker?> GetLockerByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<int> GetLockerCountBySpecification(ISpecification<Locker> specification)
        {
            return await GetCountBySpecificationAsync(specification);
        }

        public async Task<IReadOnlyList<Locker>> GetLockersBySpecificationAsync(ISpecification<Locker> specification)
        {
            return await GetBySpecificationAsync(specification);
        }

        public async Task<Locker?> GetOneLockerBySpecification(ISpecification<Locker> specification)
        {
            return await GetOneBySpecificationAsync(specification);
        }

        public async Task UpdateLockerAsync(Locker locker)
        {
            await UpdateAsync(locker);
        }
    }
}