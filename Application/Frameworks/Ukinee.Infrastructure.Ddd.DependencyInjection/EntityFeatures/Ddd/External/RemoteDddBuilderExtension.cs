using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.Synchronization.Implementations;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;

public static class RemoteDddBuilderExtension
{
    extension<TTag, TParams, TIdentifier, TEntity, TResponse>(IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse> mapServiceRegisterer)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TParams : IRouteParams<TParams, TIdentifier>
    where TResponse : notnull
    {
        public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> WithMapsterResponseToEntityMap()
        {
            return mapServiceRegisterer.WithMapper<MapsterMapService<TResponse,TEntity>>();
        }
    }
}
