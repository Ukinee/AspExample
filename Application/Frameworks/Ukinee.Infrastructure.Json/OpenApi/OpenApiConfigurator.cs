using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Ukinee.Infrastructure.Json.OpenApi;

public class JsonOpenApiConfigurator
{
    public static void FixFlagEnums(OpenApiOptions openApiOptions)
    {
        openApiOptions.AddSchemaTransformer((schema, context, _) =>
            {
                var type = context.JsonTypeInfo.Type;
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

                if (!underlyingType.IsEnum || !underlyingType.IsDefined(typeof(FlagsAttribute), inherit: false))
                    return Task.CompletedTask;

                schema.Type = JsonSchemaType.Array;

                var enumStringValues = Enum
                    .GetNames(underlyingType)
                    .Where(name =>
                        {
                            var val = Convert.ToUInt64(Enum.Parse(underlyingType, name));

                            return val != 0 && (val & val - 1) == 0;
                        }
                    )
                    .Select(JsonNode (name) => JsonValue.Create(name))
                    .ToList();

                schema.Items = new OpenApiSchema {
                    Type = JsonSchemaType.String,
                    Enum = enumStringValues,
                };

                schema.Enum = null;

                return Task.CompletedTask;
            }
        );
    }
}
