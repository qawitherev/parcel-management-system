using System.Reflection.Metadata.Ecma335;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ParcelManagement.Api.Swagger
{
    public class SwaggerVersionParameterRemover : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters.Count < 1) return;
            var versionParam = operation.Parameters.SingleOrDefault(p => p.Name == "version");
            if (versionParam == null) return;
            operation.Parameters.Remove(versionParam);
        }
    }
}