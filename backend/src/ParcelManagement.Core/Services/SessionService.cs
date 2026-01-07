using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model;
using ParcelManagement.Core.Repositories;

namespace ParcelManagement.Core.Services
{
    public interface ISessionService
    {
        Task<Session> CreateSessionAsync(CreateSessionRequest sessionRequest);

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
    }
}