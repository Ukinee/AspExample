using Ukinee.Infrastructure.Ddd.Common.Entities;
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
            return mapServiceRegisterer.WithMapper<MapsterMapService<TResponse,TEntity>>();
        }
    }
}
