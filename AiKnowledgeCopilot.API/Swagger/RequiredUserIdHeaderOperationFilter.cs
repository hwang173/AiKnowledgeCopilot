using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AiKnowledgeCopilot.API.Swagger;

public class RequiredUserIdHeaderOperationFilter : IOperationFilter
{
    private const string UserIdHeaderName = "X-User-Id";

    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        var relativePath =
            context.ApiDescription.RelativePath;

        if (string.IsNullOrWhiteSpace(relativePath) ||
            !relativePath.StartsWith(
                "documents",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        operation.Parameters ??=
            new List<OpenApiParameter>();

        var alreadyExists =
            operation.Parameters.Any(x =>
                x.In == ParameterLocation.Header &&
                string.Equals(
                    x.Name,
                    UserIdHeaderName,
                    StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return;
        }

        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = UserIdHeaderName,
                In = ParameterLocation.Header,
                Required = true,
                Description =
                    "Development user id. This will later be replaced by JWT authentication.",
                Schema =
                    new OpenApiSchema
                    {
                        Type = "string",
                        Example = new OpenApiString("user-1")
                    }
            });
    }
}