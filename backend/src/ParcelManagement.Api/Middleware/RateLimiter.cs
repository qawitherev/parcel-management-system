

using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using ParcelManagement.Core.Model.Configuration;


/**
    extension class in c#
    requirements: 
    1. static class 
    2. static method 
    3. first param of method uses this with type you want to extend
**/
namespace ParcelManagement.Api.Middleware
{
    public class RateLimiterConfiguration : IConfigureOptions<RateLimiterOptions>
    {
        const int LIMIT_FALLBACK = 10;
        const int WINDOW_TIME_FALLBACK = 1; // in minutes 
        const int SEGMENTS_FALLBACK = 10;

        private readonly RateLimitSettings _settings; 

        public RateLimiterConfiguration(IOptionsMonitor<RateLimitSettings> options)
        {
            _settings = options.CurrentValue;
        }
        public void Configure(RateLimiterOptions options)
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // TODO: to log warning when one of the valuen inside RateLimitSettings is 0

                // global rate limit 
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter("Global", _  => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = _settings.Global.PermitLimit == 0 ? LIMIT_FALLBACK : _settings.Global.PermitLimit, 
                        Window = TimeSpan.FromMinutes(_settings.Global.WindowMinutes == 0 ? WINDOW_TIME_FALLBACK : _settings.Global.WindowMinutes), 
                        SegmentsPerWindow = _settings.Global.SegmentsPerWindow == 0 ? SEGMENTS_FALLBACK : _settings.Global.SegmentsPerWindow
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
                                            PermitLimit = _settings.User.PermitLimit == 0 ? LIMIT_FALLBACK : _settings.User.PermitLimit,
                                            Window = TimeSpan.FromMinutes(_settings.User.WindowMinutes == 0 ? WINDOW_TIME_FALLBACK : _settings.User.WindowMinutes),
                                            SegmentsPerWindow = _settings.User.SegmentsPerWindow == 0 ? SEGMENTS_FALLBACK : _settings.User.SegmentsPerWindow
                                        });  });

                // endpoint based Rate limit 
                options.AddPolicy("StrictEndpointPolicy", httpContext => 
                    RateLimitPartition.GetSlidingWindowLimiter("Strict", _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = _settings.StrictEndpoint.PermitLimit == 0 ? LIMIT_FALLBACK : _settings.StrictEndpoint.PermitLimit,
                        Window = TimeSpan.FromMinutes(_settings.StrictEndpoint.WindowMinutes == 0 ? WINDOW_TIME_FALLBACK : _settings.StrictEndpoint.WindowMinutes),
                        SegmentsPerWindow = _settings.StrictEndpoint.SegmentsPerWindow == 0 ? SEGMENTS_FALLBACK : _settings.StrictEndpoint.SegmentsPerWindow
                    })
                );  }
    }
}