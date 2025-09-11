using System.Security.Claims;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Api.Utility
{
    public interface IUserContextService
    {
        Guid GetUserId();
        UserRole GetUserRole();
        string? GetClaimByClaimType(string claimType);
    }

    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string? GetClaimByClaimType(string claimType)
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        }

        public Guid GetUserId()
        {
            var u = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?
                .Value;
            if (string.IsNullOrEmpty(u) || !Guid.TryParse(u, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid user id");
            }
            return userId;
        }

        public UserRole GetUserRole()
        {
            var roleString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ??
                throw new UnauthorizedAccessException("User role missing");
            if (!Enum.TryParse<UserRole>(roleString, out var role))
            {
                throw new InvalidOperationException($"Invalid role found in claim");
            }
            return role;
        }
    }
}