using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Json.Contracts;
using Ukinee.Infrastructure.Json.Models;
using Ukinee.Infrastructure.Json.Services;

namespace Ukinee.Infrastructure.Json.Extensions.ServiceCollections;

public static partial class JsonServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddJsonProvider<TTag>(Action<JsonSerializerOptions>? configureOptions = null)
        {
            services.AddSingleton<IJsonOptionsProvider<TTag>>(sp =>
                {
                    var mappings = sp.GetServices<PolymorphicMapping>();

                    return new JsonOptionsProvider<TTag>(mappings, configureOptions);
                }
            );

            return services;
        }

        public IServiceCollection AddPolymorphicTypes(
            Type[] baseTypes,
            Assembly[]? assembliesToScan = null,
            string typeDiscriminator = "$type"
        )
        {
            var assemblies = assembliesToScan ?? baseTypes.Select(t => t.Assembly).ToArray();

            foreach (var baseType in baseTypes)
            {
                var derivedTypes = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType));

                foreach (var derived in derivedTypes)
                {
                    services.AddSingleton(new PolymorphicMapping(baseType, derived, typeDiscriminator, derived.Name));
                }
            }

            return services;
        }
    }
}
