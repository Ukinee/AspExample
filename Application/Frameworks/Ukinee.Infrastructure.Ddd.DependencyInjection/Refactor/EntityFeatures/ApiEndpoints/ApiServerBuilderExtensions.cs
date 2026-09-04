using Microsoft.AspNetCore.Builder;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.ApiEndpoints;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;

// ReSharper disable once CheckNamespace
namespace Ukinee.AspNetCore.ModuleRegisterer.Refactor.EntityFeatures.ApiEndpointFeature;

public static class ApiServerExtensions
{
    extension<TIdentifier, TEntity, TParams, TResponse>(ICrudGroupConfigurator<TIdentifier, TEntity, TParams, TResponse> configurator)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    where TParams : struct, IRouteParams<TParams, TIdentifier>
    {
        public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> ConfigureDebugAuthorization()
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{typeof(TEntity).Name} uses DebugAuthorization.");
            Console.ForegroundColor = color;

            return configurator.WithGroupConfiguration(builder => { });
        }
    }
}
