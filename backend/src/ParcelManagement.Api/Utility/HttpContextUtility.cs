using System.Security.Claims;
using ParcelManagement.Core.Entities;
using UAParser;

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

    public static class HttpContextUtilities
    {
        public static string GetDeviceInfo(HttpContext context)
        {
            var nonParsed = context.Request.Headers.UserAgent.ToString();
            var parser = Parser.GetDefault();
            var client = parser.Parse(nonParsed);
            if (client != null)
            {
                return $"{client.UA.Family} on {client.OS.Family}";
            }
            return "";
        }

        public static string GetDeviceIp(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(ip))
            {
                return ip.Split(",")[0].Trim();
            }
            return context.Connection.RemoteIpAddress?.ToString() ?? "";
        }
    }

}