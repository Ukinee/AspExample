using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;

public class LocalCacheRequestHandler<TIdentifier, TEntity>(IEntityCache<TIdentifier, TEntity> cache) :
    IRequestHandler<UpsertCachedEntitiesCommand<TIdentifier, TEntity>>,
    IRequestHandler<InvalidateCacheCommand<TIdentifier, TEntity>>,
    IRequestHandler<RemoveCachedEntitiesCommand<TIdentifier, TEntity>>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    public async Task Handle(UpsertCachedEntitiesCommand<TIdentifier, TEntity> request, CancellationToken cancellationToken)
    {
        cache.Upsert(request.Entities);
    }

    public async Task Handle(InvalidateCacheCommand<TIdentifier, TEntity> request, CancellationToken cancellationToken)
    {
        cache.Invalidate(request.Identifiers);
    }

    public async Task Handle(RemoveCachedEntitiesCommand<TIdentifier, TEntity> request, CancellationToken cancellationToken)
    {
        cache.MarkNotExists(request.Identifiers);
    }
}

public class VoidingLocalCacheRequestHandler<TIdentifier, TEntity> :
    IRequestHandler<UpsertCachedEntitiesCommand<TIdentifier, TEntity>>,
    IRequestHandler<InvalidateCacheCommand<TIdentifier, TEntity>>,
    IRequestHandler<RemoveCachedEntitiesCommand<TIdentifier, TEntity>>
where TEntity : IEntity<TIdentifier>
{
    public async Task Handle(UpsertCachedEntitiesCommand<TIdentifier, TEntity> request, CancellationToken cancellationToken) { }
    public async Task Handle(InvalidateCacheCommand<TIdentifier, TEntity> request, CancellationToken cancellationToken) { }
    public async Task Handle(RemoveCachedEntitiesCommand<TIdentifier, TEntity> request, CancellationToken cancellationToken) { }
}
