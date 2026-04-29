using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ERP.API.Helpers
{
    /// <summary>
    /// Swagger schema filter that replaces the default "string" example
    /// with an empty string for all string-type properties.
    /// </summary>
    public class EmptyStringSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Type == "string" && schema.Example is OpenApiString example && example.Value == "string")
            {
                schema.Example = new OpenApiString("");
            }

            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties.Values)
                {
                    if (property.Type == "string" && property.Example is OpenApiString propExample && propExample.Value == "string")
                    {
                        property.Example = new OpenApiString("");
                    }
                }
            }
        }
    }
}
