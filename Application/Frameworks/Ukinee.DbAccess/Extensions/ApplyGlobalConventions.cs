using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ukinee.DbAccess.Extensions;

public static class EntityConfigurationExtensions
{
    public static void ApplyGlobalConventions(this ModelConfigurationBuilder builder)
    {
        builder.ApplyIdentifierConverters();
        
        builder
            .Properties<JsonObject>()
            .HaveConversion<JsonObjectToStringConverter>();

        builder
            .Properties<float[]>()
            .HaveConversion<VectorValueConverter, VectorValueComparer>();

        builder
            .Properties<float[]?>()
            .HaveConversion<VectorValueConverter, VectorValueComparer>();

        builder
            .Properties<Enum>()
            .HaveConversion<int>();
    }
}

public class JsonObjectToStringConverter : ValueConverter<JsonObject?, string?>
{
    public JsonObjectToStringConverter() : base(
        obj => obj == null ? null : obj.ToJsonString(null),
        str => string.IsNullOrEmpty(str) ? null : JsonNode.Parse(str, null, default)!.AsObject()
    ) { }
}
