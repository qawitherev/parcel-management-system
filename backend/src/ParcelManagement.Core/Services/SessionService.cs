using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface ISessionService
    {
        Task<Session> CreateSessionAsync(CreateSessionRequest sessionRequest);
        Task<Session?> GetSessionBySpecification(ISpecification<Session> specification);
        Task UpdateSession(Session session);
        Task RemoveSession(Guid id);

    }

    public class SessionService(ISessionRepository sessionRepo) : ISessionService
    {

        const int USER_MAX_SESSION = 5;
        private readonly ISessionRepository _sessionRepo = sessionRepo;

        public async Task<Session> CreateSessionAsync(CreateSessionRequest sessionRequest)
        {
            var sessionByUserSpec = new SessionByUserSpecification(sessionRequest.UserId, USER_MAX_SESSION-1);
            var sessions = await _sessionRepo.GetSessionsBySpecification(sessionByUserSpec);

            var sessionIds = sessions.Select(s => s.Id);
            await _sessionRepo.DeleteSessionsAsync(sessionIds);
            
            var session = new Session
            {
                Id = Guid.NewGuid(), 
                UserId = sessionRequest.UserId, 
                RefreshToken = sessionRequest.RefreshToken,
                DeviceInfo = sessionRequest.DeviceInfo, 
                IpAddress = sessionRequest.IpAddress, 
                ExpiresAt = sessionRequest.ExpiresAt, 
                LastActive = DateTimeOffset.UtcNow
            };
            return await _sessionRepo.CreateSessionAsync(session);

        }

        public async Task<Session?> GetSessionBySpecification(ISpecification<Session> specification)
        {
            return await _sessionRepo.GetSessionBySpecification(specification);
        }

        public async Task RemoveSession(Guid userId)
        {
            var spec = new SessionByUserSpecification(userId, null);
            var session = await _sessionRepo.GetSessionBySpecification(spec) ?? 
                throw new InvalidOperationException($"Session not found");
            await _sessionRepo.DeleteSessionAsync(session.Id);
        }

        public async Task UpdateSession(Session session)
        {
            await _sessionRepo.UpdateSessionAsync(session);
        }
    }
}