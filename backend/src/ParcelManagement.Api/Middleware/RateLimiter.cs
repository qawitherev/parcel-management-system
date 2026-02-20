

using System.Security.Claims;
using System.Threading.RateLimiting;


/**
    extension class in c#
    requirements: 
    1. static class 
    2. static method 
    3. first param of method uses this with type you want to extend
**/
namespace ParcelManagement.Api.Middleware
{
    public static class RateLimitExtensions
    {
        public static IServiceCollection AddRateLimit(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // global rate limit 
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext => 
                    RateLimitPartition.GetSlidingWindowLimiter("Global", _  => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100, 
                        Window = TimeSpan.FromMinutes(1), 
                        SegmentsPerWindow = 10
                    })
                );

                // user based rate limit
                options.AddPolicy("UserPolicy", httpContext =>
                {
                    var userId = httpContext.User.FindFirstValue(ClaimTypes.Name) 
                        ?? httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "Anonymous";
                    return RateLimitPartition.GetSlidingWindowLimiter(userId, _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10, 
                        Window = TimeSpan.FromMinutes(1), 
                        SegmentsPerWindow = 10
                    });
                });

                // endpoint based Rate limit 
                options.AddPolicy("StrictEndpointPolicy", httpContext => 
                    RateLimitPartition.GetSlidingWindowLimiter("Strict", _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 75, 
                        Window = TimeSpan.FromMinutes(1), 
                        SegmentsPerWindow = 3
                    })
                );
            });
            return services; 
        }
    }
}