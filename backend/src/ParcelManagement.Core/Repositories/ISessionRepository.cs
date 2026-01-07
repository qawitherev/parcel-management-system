using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface ISessionRepository
    {
        Task<Session> CreateSessionAsync(Session session);
        Task<Session?> GetSessionByIdAsync(Guid id);
        Task UpdateSessionAsync(Session session);
        Task<Session?> GetSessionBySpecification(ISpecification<Session> specification);
        Task<IReadOnlyList<Session>> GetSessionsBySpecification(ISpecification<Session> specification);
        Task<int> DeleteSessionAsync(Guid id);
        Task<int> DeleteSessionsAsync(IEnumerable<Guid> ids);
    }
}