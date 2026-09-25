using System.Runtime.CompilerServices;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;

public class CachingEntityReader<TIdentifier, TEntity, TInner>(
    IEntityCache<TIdentifier, TEntity> cache,
    TInner inner
) : IEntityReader<TIdentifier, TEntity>
where TIdentifier : struct
where TEntity : class, IEntity<TIdentifier>
where TInner : class, IEntityReader<TIdentifier, TEntity>
{
    public async IAsyncEnumerable<TEntity> GetAllAsync(UserContext userContext, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        cache.Clear();

        var innerStream = inner.GetAllAsync(userContext, cancellationToken);

        await foreach (var entity in innerStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            cache.Upsert(entity);

            yield return entity;
        }
    }
}

public class CachingIdentifierReader<TIdentifier, TEntity, TInner>(
    IEntityCache<TIdentifier, TEntity> cache,
    TInner inner
) : IIdentifierReader<TIdentifier, TEntity>
where TIdentifier : struct
where TEntity : class, IEntity<TIdentifier>
where TInner : class, IIdentifierReader<TIdentifier, TEntity>
{
    public async Task<TEntity?> FindByIdAsync(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken)
    {
        var lookup = cache.Lookup(identifier);

        switch (lookup.Status)
        {
            case CacheLookupStatus.Unknown:
                var innerResult = await inner.FindByIdAsync(userContext, identifier, cancellationToken);

                if (innerResult == null)
                {
                    cache.MarkNotExists(identifier);

                    return null;
                }

                cache.Upsert(innerResult);

                return innerResult;

            case CacheLookupStatus.Hit: return lookup.Entity;
            case CacheLookupStatus.KnownMissing: return null;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public async IAsyncEnumerable<TEntity> FindManyByIdAsync(
        UserContext userContext,
        IReadOnlyCollection<TIdentifier> identifiers,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var unknownIds = new List<TIdentifier>();

        foreach (var identifier in identifiers)
        {
            var lookup = cache.Lookup(identifier);

            switch (lookup.Status)
            {
                case CacheLookupStatus.Hit: yield return lookup.Entity!; break;
                case CacheLookupStatus.Unknown: unknownIds.Add(identifier); break;

                case CacheLookupStatus.KnownMissing:
                default:
                    break;
            }
        }

        if (unknownIds.Count == 0)
            yield break;

        var foundIds = new HashSet<TIdentifier>();

        await foreach (var entity in inner.FindManyByIdAsync(userContext, unknownIds, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            cache.Upsert(entity);
            foundIds.Add(entity.Identifier);

            yield return entity;
        }

        var notFoundIds = unknownIds.Where(id => !foundIds.Contains(id)).ToArray();

        if (notFoundIds.Length > 0)
            cache.MarkNotExists(notFoundIds);
    }

    public async Task<IReadOnlyDictionary<TIdentifier, TEntity>> GetManyByIdAsync(
        UserContext userContext,
        IReadOnlyCollection<TIdentifier> identifiers,
        CancellationToken cancellationToken
    )
    {
        var innerResult = await inner.GetManyByIdAsync(userContext, identifiers, cancellationToken);

        foreach ((_, var entity) in innerResult)
            cache.Upsert(entity);

        return innerResult;
    }
}
