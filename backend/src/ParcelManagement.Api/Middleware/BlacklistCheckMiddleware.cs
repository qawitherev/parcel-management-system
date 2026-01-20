using System.Security.Authentication;
using ParcelManagement.Api.CustomAttribute;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Middleware
{
    public class BlacklistCheckMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklistService1, IUserContextService userContextService1)
        {
            var metadata = context.GetEndpoint()?.Metadata.GetMetadata<SkipBlacklistCheckAttribute>();
            if (metadata != null)
            {
                await _next(context);
                return;
            }
            var jti = userContextService1.GetTokenId();
            var isBlacklisted = await tokenBlacklistService1.IsTokenBlacklisted(jti ?? "");
            if (isBlacklisted)
            {
                throw new InvalidCredentialException($"Unauthorized access");
            } else
            {
                await _next(context);
            }
        }
    }
}