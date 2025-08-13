using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using ParcelManagement.Core.Exceptions;

namespace ParcelManagement.Api.Middleware
{
    public class ApiExceptionMiddelware(RequestDelegate next, ILogger<ApiExceptionMiddelware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ApiExceptionMiddelware> _logger = logger;

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                var response = httpContext.Response;
                response.ContentType = "application/json";
                response.StatusCode = ex switch
                {
                    ApiException e => e.StatusCode,
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    InvalidOperationException => (int)HttpStatusCode.Conflict,
                    InvalidCredentialException => (int)HttpStatusCode.Unauthorized,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                var result = JsonSerializer.Serialize(new
                {
                    message = ex.Message,
                    statusCode = response.StatusCode
                });

                _logger.LogError(ex, ex.Message);
                await response.WriteAsync(result);
            }
        }
    }
}