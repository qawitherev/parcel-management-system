using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ParcelManagement.Api.AuthenticationAndAuthorization
{
    public class JwtBearerAccessEvent
    {
        public static JwtBearerEvents UnauthorizedOrForbiddenAccessEvent()
        {
            return new JwtBearerEvents
            {
                OnForbidden = async (context) =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = "Forbidden",
                        message = "You do not have permission to access this resource",
                        timestamp = DateTime.UtcNow,
                        path = context.HttpContext.Request.Path
                    }));
                },

                OnChallenge = async (context) =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Unauthorized",
                        message = "Invalid access credentials provided",
                        timestamp = DateTime.UtcNow, 
                        path = context.HttpContext.Request.Path
                    });
                }
            };
        }
    }
}