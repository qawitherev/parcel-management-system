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
            catch (OperationCanceledException ex)
            {
                if (httpContext.RequestAborted.IsCancellationRequested)
                {
                    return;
                }
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.GatewayTimeout);
            }

            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex, HttpStatusCode.InternalServerError);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex, HttpStatusCode defaultCode)
        {
            if (httpContext.Response.HasStarted) return;
            
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = ex switch
            {
                ApiException e => e.StatusCode,
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    NullReferenceException => (int)HttpStatusCode.NotFound,
                    InvalidOperationException => (int)HttpStatusCode.Conflict,
                    InvalidCredentialException => (int)HttpStatusCode.Unauthorized,
                    _ => (int)defaultCode
            };

            var responseBody = JsonSerializer.Serialize(new
            {
                message = ex.Message
            });
            await httpContext.Response.WriteAsync(responseBody);
        }
    }
}