using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Local.Repositories;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
public class InMemoryDictionaryRepository<TIdentifier, TEntity>
    : RepositoryBase<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    protected readonly ConcurrentDictionary<TIdentifier, TEntity> Collection = new();

#region Edit

    public override Task<UpdateResult<TEntity>> UpdateByIdAsync(
        TIdentifier identifier,
        UpdateLock mode,
        Expression<Func<TEntity, bool>> filter,
        Func<TEntity, TEntity> updateFactory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var filterFunc = filter.Compile();
        TEntity? previous = null;

        var current = Collection.AddOrUpdate(
            identifier,
            _ => throw new EntityNotFoundException<TIdentifier, TEntity>(identifier),
            (_, old) =>
            {
                if (!filterFunc(old))
                    throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

                previous = old;
                return updateFactory(old);
            });

        return Task.FromResult(new UpdateResult<TEntity>(current, previous!));
    }

    public override Task<IReadOnlyList<UpdateResult<TEntity>>> UpdateManyByIdAsync(
        IReadOnlyCollection<TIdentifier> identifiers,
        UpdateLock mode,
        Expression<Func<TEntity, bool>> filter,
        Func<TIdentifier, TEntity, TEntity> update,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var filterFunc = filter.Compile();
        var idList = identifiers as IReadOnlyList<TIdentifier> ?? identifiers.ToArray();

        var previous = new TEntity[idList.Count];

        for (var i = 0; i < idList.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Collection.TryGetValue(idList[i], out var entity) || !filterFunc(entity))
                throw new EntityNotFoundException<TIdentifier, TEntity>(idList[i]);

            previous[i] = entity;
        }

        var current = new TEntity[idList.Count];

        for (var i = 0; i < idList.Count; i++)
            current[i] = update(idList[i], previous[i]);

        var results = new List<UpdateResult<TEntity>>(idList.Count);

        for (var i = 0; i < idList.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Collection.TryUpdate(idList[i], current[i], previous[i]))
                throw new EntityNotFoundException<TIdentifier, TEntity>(idList[i]);

            results.Add(new UpdateResult<TEntity>(current[i], previous[i]));
        }

        return Task.FromResult<IReadOnlyList<UpdateResult<TEntity>>>(results);
    }

    protected override Task AddRangeInternal(IReadOnlyCollection<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (!Collection.TryAdd(entity.Identifier, entity))
                throw new EntityAlreadyExistsException<TIdentifier, TEntity>(entity.Identifier);
        }

        return Task.CompletedTask;
    }

    protected override Task<IReadOnlyCollection<TEntity>> RemoveRangeInternal(
        IEnumerable<TIdentifier> identifiers,
        Expression<Func<TEntity, bool>> filter)
    {
        var filterFunc = filter.Compile();
        var idList = identifiers as IReadOnlyList<TIdentifier> ?? identifiers.ToArray();

        for (var i = 0; i < idList.Count; i++)
            if (!Collection.TryGetValue(idList[i], out var entity) || !filterFunc(entity))
                throw new EntityNotFoundException<TIdentifier, TEntity>(idList[i]);

        var result = new List<TEntity>(idList.Count);

        for (var i = 0; i < idList.Count; i++)
        {
            if (!Collection.TryRemove(idList[i], out var entity))
                throw new EntityNotFoundException<TIdentifier, TEntity>(idList[i]);

            result.Add(entity);
        }

        return Task.FromResult<IReadOnlyCollection<TEntity>>(result);
    }

#endregion

#region Read

    public override Task<TEntity?> FindByIdAsync(
        TIdentifier identifier,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Collection.TryGetValue(identifier, out var entity))
            return Task.FromResult<TEntity?>(null);

        var filterFunc = filter.Compile();

        return Task.FromResult(filterFunc(entity) ? entity : null);
    }

    public override Task<TEntity?> FindAsync(
        Specification<TEntity> specification,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        var filterFunc = filter.Compile();

        foreach (var pair in Collection)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var value = pair.Value;

            if (filterFunc(value) && specification.IsSatisfiedBy(value))
                return Task.FromResult<TEntity?>(value);
        }

        return Task.FromResult<TEntity?>(null);
    }

    public override IAsyncEnumerable<TEntity> FindManyAsync(
        Specification<TEntity> specification,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        var filterFunc = filter.Compile();

        var result = specification.Evaluate(ToFilteredValuesNonAlloc(cancellationToken));

        return result.ToAsyncEnumerable();

        IEnumerable<TEntity> ToFilteredValuesNonAlloc(CancellationToken token)
        {
            foreach (var pair in Collection)
            {
                if (token.IsCancellationRequested)
                    yield break;

                var value = pair.Value;

                if (filterFunc(value))
                    yield return value;
            }
        }
    }

    public override async IAsyncEnumerable<TEntity> FindManyByIdAsync(
        IEnumerable<TIdentifier> identifiers,
        Expression<Func<TEntity, bool>> filter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var filterFunc = filter.Compile();

        foreach (var identifier in identifiers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Collection.TryGetValue(identifier, out var entity) && filterFunc(entity))
                yield return entity;
        }
    }

#endregion
}