using System.Security.Claims;

namespace ParcelManagement.Api.Utility
{
    public interface IUserContextService
    {
        Guid GetUserId();
        string GetUserRole();
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

        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ??
                throw new UnauthorizedAccessException("User role missing");
        }
    }
}