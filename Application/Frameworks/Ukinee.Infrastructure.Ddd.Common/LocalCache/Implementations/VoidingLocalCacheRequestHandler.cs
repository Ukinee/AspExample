using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;

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
