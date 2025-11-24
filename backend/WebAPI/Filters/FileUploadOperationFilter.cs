using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if this is the upload endpoint by checking the route
        var actionDescriptor = context.ApiDescription.ActionDescriptor;
        var routeTemplate = context.ApiDescription.RelativePath;

        if (routeTemplate == null || !routeTemplate.Contains("upload"))
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["file"] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary",
                                Description = "The file to upload",
                            },
                            ["content"] = new OpenApiSchema
                            {
                                Type = "string",
                                Nullable = true,
                                Description = "Optional text content",
                            },
                        },
                        Required = new HashSet<string> { "file" }, // Only file is required
                    },
                },
            },
        };

        // Remove content from query parameters if it exists
        if (operation.Parameters != null)
        {
            var contentParam = operation.Parameters.FirstOrDefault(p => p.Name == "content");
            if (contentParam != null)
            {
                operation.Parameters.Remove(contentParam);
            }
        }
    }
}
