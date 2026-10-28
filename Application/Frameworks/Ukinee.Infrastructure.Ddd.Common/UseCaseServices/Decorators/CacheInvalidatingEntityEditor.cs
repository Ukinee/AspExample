using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Decorators;

public class CacheInvalidatingEntityCreatorDecorator<TIdentifier, TEntity, TCreatePayload>(
    IEntityCreator<TEntity, TCreatePayload> inner,
    IEntityCache<TIdentifier, TEntity> cache
) : IEntityCreator<TEntity, TCreatePayload>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    public async Task<TEntity> CreateAsync(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken)
    {
        var result = await inner.CreateAsync(userContext, payload, cancellationToken);

        cache.Invalidate(result.Identifier);

        return result;
    }

    public async Task<IReadOnlyCollection<TEntity>> CreateAsync(
        UserContext userContext,
        IReadOnlyCollection<TCreatePayload> payloads,
        CancellationToken cancellationToken
    )
    {
        var result = await inner.CreateAsync(userContext, payloads, cancellationToken);

        cache.Invalidate(result.Select(e => e.Identifier));

        return result;
    }
}

public class CacheInvalidatingUpdaterDecorator<TIdentifier, TEntity, TPayload>(
    IEntityPayloadUpdater<TIdentifier, TEntity, TPayload> inner,
    IEntityCache<TIdentifier, TEntity> cache
) : IEntityPayloadUpdater<TIdentifier, TEntity, TPayload>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    public async Task<TEntity> UpdateAsync(UserContext userContext, TIdentifier identifier, TPayload payload, CancellationToken cancellationToken)
    {
        var result = await inner.UpdateAsync(userContext, identifier, payload, cancellationToken);

        cache.Invalidate(result.Identifier);

        return result;
    }

    public async Task<IReadOnlyCollection<TEntity>> UpdateAsync(
        UserContext userContext,
        IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        var result = await inner.UpdateAsync(userContext, payloads, cancellationToken);

        cache.Invalidate(result.Select(e => e.Identifier));

        return result;
    }
}

public class CacheInvalidatingEntityRemoverDecorator<TIdentifier, TEntity>(
    IEntityRemover<TIdentifier, TEntity> inner,
    IEntityCache<TIdentifier, TEntity> cache
) : IEntityRemover<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    public async Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        await inner.RemoveAsync(userContext, entities, cancellationToken);

        cache.Invalidate(entities.Select(e => e.Identifier));
    }

    public async Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        await inner.RemoveAsync(userContext, identifiers, cancellationToken);

        cache.Invalidate(identifiers);
    }
}

public class CacheInvalidatingGetOrCreateDecorator<TIdentifier, TEntity, TPayload>(
    IEntityEnsureExistsCreator<TIdentifier, TEntity, TPayload> inner,
    IEntityCache<TIdentifier, TEntity> cache
) : IEntityEnsureExistsCreator<TIdentifier, TEntity, TPayload>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    public async Task<TEntity> GetOrCreateAsync(UserContext userContext, TIdentifier identifier, TPayload payload, CancellationToken cancellationToken)
    {
        var result = await inner.GetOrCreateAsync(userContext, identifier, payload, cancellationToken);

        cache.Invalidate(result.Identifier);

        return result;
    }

    public async Task<IReadOnlyCollection<TEntity>> GetOrCreateAsync(
        UserContext userContext,
        IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TPayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        var result = await inner.GetOrCreateAsync(userContext, payloads, cancellationToken);

        cache.Invalidate(result.Select(e => e.Identifier));

        return result;
    }
}
