using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;

// ReSharper disable once CheckNamespace
namespace Ukinee.AspNetCore.ModuleRegisterer.Refactor.EntityFeatures.ApiEndpointFeature;

public static class ApiServerExtensions
{
    extension<TIdentifier, TEntity, TResponse>(ICrudGroupConfigurator<TIdentifier, TEntity, TResponse> configurator)
    where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
    where TEntity : class, IEntity<TIdentifier>
    {
        public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithNoAdditionalConfiguration()
        {
            return configurator.WithGroupConfiguration(builder => { });
        }
    }
}
