using Microsoft.Extensions.Options;
using ParcelManagement.Api.AuthenticationAndAuthorization;
using ParcelManagement.Core.Model;
using ParcelManagement.Core.Repositories;

namespace ParcelManagement.Core.Services
{
    public interface ITokenBlacklistService
    {
        Task<bool> BlacklistAccessToken(string jwtId);
        Task<bool> IsTokenBlacklisted(string jwtId);
    }

    public class TokenBlacklistService(IRedisRepository redisRepo, IOptions<JWTSettings> options) : ITokenBlacklistService
    {
        private readonly IRedisRepository _redisRepo = redisRepo;
        private readonly JWTSettings _jwtSettings = options.Value;

        const string KEY_PREFIX = "blacklisted-";

        public async Task<bool> BlacklistAccessToken(string jwtId)
        {
            return await _redisRepo.SetValueAsync(
                $"{KEY_PREFIX}{jwtId}",
                "blacklisted",
                TimeSpan.FromMinutes(Convert.ToDouble(_jwtSettings.ExpirationMinutes ?? 0))
            );
        }

        public async Task<bool> IsTokenBlacklisted(string jwtId)
        {
            var key = $"{KEY_PREFIX}{jwtId}";
            return await _redisRepo.KeyExistAsync(key);
        }
    }
}