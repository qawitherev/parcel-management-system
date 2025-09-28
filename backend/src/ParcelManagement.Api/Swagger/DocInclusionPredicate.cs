using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ParcelManagement.Api.Swagger
{
    public static class SwaggerSetup
    {
        public static bool DocInclusionPredicate(string docName, ApiDescription apiDesc)
        {
            // reflection to get the controller method 
            if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;

            var versions = methodInfo?.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<ApiVersionAttribute>()
                .SelectMany(attr => attr.Versions)
                .ToList();

            return versions!.Any(v => $"v{v.MajorVersion}" == docName);
        }
    }
}