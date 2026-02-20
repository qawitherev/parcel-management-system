

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
                        SegmentsPerWindow = 5
                    })
                );
            });
            return services; 
        }
    }
}