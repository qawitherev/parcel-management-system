using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class SessionRepository(ApplicationDbContext dbContext) : BaseRepository<Session>(dbContext), ISessionRepository
    {
        public async Task<Session> CreateSessionAsync(Session session)
        {
            await CreateAsync(session);
            return session; 
        }

        public async Task<int> DeleteSessionAsync(Guid id)
        {
            return await DeleteAsync(id);
        }

        public async Task<int> DeleteSessionsAsync(IEnumerable<Guid> ids)
        {
            return await DeleteRangeAsync(ids);
        }

        public async Task<Session?> GetSessionByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<Session?> GetSessionBySpecification(ISpecification<Session> specification)
        {
            return await GetOneBySpecificationAsync(specification);
        }

        public async Task<IReadOnlyList<Session>> GetSessionsBySpecification(ISpecification<Session> specification)
        {
            return await GetBySpecificationAsync(specification);
        }

        public async Task UpdateSessionAsync(Session session)
        {
            await UpdateAsync(session);
        }
    }
}