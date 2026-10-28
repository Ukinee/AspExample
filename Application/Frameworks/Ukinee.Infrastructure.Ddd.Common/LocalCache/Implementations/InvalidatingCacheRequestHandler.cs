using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;

public class InvalidatingCacheRequestHandler<TIdentifier, TEntity>(IEntityCache<TIdentifier, TEntity> cache) :
    IRequestHandler<UpsertCachedEntitiesCommand<TIdentifier, TEntity>>,
    IRequestHandler<InvalidateCacheCommand<TIdentifier, TEntity>>,
    IRequestHandler<RemoveCachedEntitiesCommand<TIdentifier, TEntity>>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    public async Task Handle(UpsertCachedEntitiesCommand<TIdentifier, TEntity> request, CancellationToken cancellationToken)
    {
        cache.Invalidate(request.Entities.Select(entity => entity.Identifier));
    }

    public async Task Handle(InvalidateCacheCommand<TIdentifier, TEntity> request, CancellationToken cancellationToken)
    {
        cache.Invalidate(request.Identifiers);
    }

    public async Task Handle(RemoveCachedEntitiesCommand<TIdentifier, TEntity> request, CancellationToken cancellationToken)
    {
        cache.Invalidate(request.Identifiers);
    }
}