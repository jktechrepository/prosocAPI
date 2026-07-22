using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProsocAPI.Swagger;

/// <summary>
/// Documente correctement les actions multipart avec plusieurs fichiers (évite la clé ContentType dupliquée).
/// </summary>
public sealed class MultipartFormFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var consumesMultipart = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<ConsumesAttribute>()
            .Any(a => a.ContentTypes.Any(t =>
                string.Equals(t, "multipart/form-data", StringComparison.OrdinalIgnoreCase)));

        if (!consumesMultipart)
            return;

        var fileParams = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Type == typeof(IFormFile) || p.Type == typeof(IFormFile))
            .ToList();

        if (fileParams.Count == 0)
            return;

        var properties = new Dictionary<string, OpenApiSchema>(StringComparer.OrdinalIgnoreCase);

        foreach (var param in context.ApiDescription.ParameterDescriptions)
        {
            if (param.Type == typeof(IFormFile) || param.Type == typeof(IFormFile))
            {
                properties[param.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
            }
            else if (param.Source.Id == "Form" || param.Source.Id == "Body")
            {
                properties[param.Name] = new OpenApiSchema { Type = "string" };
            }
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = properties,
                    },
                },
            },
        };
    }
}
