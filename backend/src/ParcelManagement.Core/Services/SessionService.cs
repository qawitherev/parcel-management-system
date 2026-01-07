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

    }

    public class SessionService(ISessionRepository sessionRepo) : ISessionService
    {
        private readonly ISessionRepository _sessionRepo = sessionRepo;
        const int REFRESH_TOKEN_EXPIRY = 10;
        public async Task<Session> CreateSessionAsync(CreateSessionRequest sessionRequest)
        {
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

        public async Task UpdateSession(Session session)
        {
            await _sessionRepo.UpdateSessionAsync(session);
        }
    }
}