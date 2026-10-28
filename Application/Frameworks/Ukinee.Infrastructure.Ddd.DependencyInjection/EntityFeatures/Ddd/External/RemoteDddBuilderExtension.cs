using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External.Services;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.Synchronization.Implementations;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;

public static class RemoteDddBuilderExtension
{
    extension<TTag, TIdentifier, TEntity, TResponse>(IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse> mapServiceRegisterer)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
    where TResponse : notnull
    {
        public RemoteDddBuilder<TTag, TIdentifier, TEntity, TResponse> WithMapsterResponseToEntityMap()
        {
            return mapServiceRegisterer.WithMapper<MapsterMapService<TResponse, TEntity>>();
        }
    }

    extension<TTag, TIdentifier, TEntity, TResponse>(ICacheRegisterer<TTag, TIdentifier, TEntity, TResponse> cacheRegisterer)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
    where TResponse : notnull
    {
        public IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse> WithNoCaching()
        {
            return cacheRegisterer.WithCache(new NoCacheRegisterer<TIdentifier, TEntity>());
        }

        public IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse> WithReadCacheAndEditingInvalidation()
        {
            return cacheRegisterer.WithCache(new ReadCacheAndEditingInvalidationRegisterer<TIdentifier, TEntity>());
        }
    }

    extension<TTag, TIdentifier, TEntity, TResponse>(ICacheRegisterer<TTag, TIdentifier, TEntity, TResponse> cacheRegisterer)
    where TEntity : class, IEntity<TIdentifier>, IEntityWithVersion<TEntity>
    where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
    where TResponse : notnull
    {
        public IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse> WithVersionedCaching()
        {
             return cacheRegisterer.WithCache(new EditingUpsertRegisterer<TIdentifier, TEntity>());
        }
    }
}
