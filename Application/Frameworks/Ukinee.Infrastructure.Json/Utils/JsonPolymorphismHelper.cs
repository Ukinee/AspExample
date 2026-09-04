using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Ukinee.Infrastructure.Json.Models;

namespace Ukinee.Infrastructure.Json.Utils;

public static class JsonPolymorphismHelper
{
    public static void ApplyMappings(JsonSerializerOptions options, IEnumerable<PolymorphicMapping> mappings)
    {
        var registry = mappings
            .GroupBy(m => m.BaseType)
            .ToDictionary(g => g.Key, g => g.ToList());

        var isOriginal = true;

        if (options.TypeInfoResolverChain.FirstOrDefault(r => r is DefaultJsonTypeInfoResolver) is not DefaultJsonTypeInfoResolver resolver)
        {
            isOriginal = false;
            resolver = new DefaultJsonTypeInfoResolver();
        }

        resolver.Modifiers.Add(typeInfo =>
            {
                if (!registry.TryGetValue(typeInfo.Type, out var maps))
                    return;

                var polymorphismOptions = typeInfo.PolymorphismOptions ?? new JsonPolymorphismOptions();
                polymorphismOptions.UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization;
                polymorphismOptions.IgnoreUnrecognizedTypeDiscriminators = false;

                if (maps.FirstOrDefault()?.DiscriminatorKey is string customKey)
                {
                    polymorphismOptions.TypeDiscriminatorPropertyName = customKey;
                }

                var newMappings = maps.Where(map => polymorphismOptions.DerivedTypes.All(d => d.DerivedType != map.DerivedType));

                foreach (var map in newMappings)
                {
                    polymorphismOptions.DerivedTypes.Add(new JsonDerivedType(map.DerivedType, map.DiscriminatorValue));
                }

                typeInfo.PolymorphismOptions = polymorphismOptions;
            }
        );

        if (!isOriginal)
        {
            options.TypeInfoResolverChain.Add(resolver);
        }
    }
}
