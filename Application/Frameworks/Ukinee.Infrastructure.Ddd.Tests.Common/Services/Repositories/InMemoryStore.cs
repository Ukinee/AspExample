using System.Collections.Concurrent;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Services.Repositories;

public class InMemoryStore<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public readonly ConcurrentDictionary<TIdentifier, TEntity> Collection = new();

#region Edit
    public UpdateResult<TEntity> UpdateById(
        TIdentifier identifier,
        Func<TEntity, bool> filter,
        Func<TEntity, TEntity> updateFactory
    )
    {
        TEntity? previous = null;

        var current = Collection.AddOrUpdate(
            identifier,
            _ => throw new EntityNotFoundException<TIdentifier, TEntity>(identifier),
            (_, old) =>
            {
                if (!filter(old))
                    throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

                previous = old;

                return updateFactory(old);
            }
        );

        return new UpdateResult<TEntity>(current, previous!);
    }

    public IReadOnlyList<UpdateResult<TEntity>> UpdateManyById(
        IReadOnlyCollection<TIdentifier> identifiers,
        Func<TEntity, bool> filter,
        Func<TIdentifier, TEntity, TEntity> update
    )
    {
        var idList = identifiers as IReadOnlyList<TIdentifier> ?? identifiers.ToArray();

        var previous = new TEntity[idList.Count];

        for (var i = 0; i < idList.Count; i++)
        {
            if (!Collection.TryGetValue(idList[i], out var entity) || !filter(entity))
                throw new EntityNotFoundException<TIdentifier, TEntity>(idList[i]);

            previous[i] = entity;
        }

        var current = new TEntity[idList.Count];

        for (var i = 0; i < idList.Count; i++)
            current[i] = update(idList[i], previous[i]);

        var results = new List<UpdateResult<TEntity>>(idList.Count);

        for (var i = 0; i < idList.Count; i++)
        {
            if (!Collection.TryUpdate(idList[i], current[i], previous[i]))
                throw new EntityNotFoundException<TIdentifier, TEntity>(idList[i]);

            results.Add(new UpdateResult<TEntity>(current[i], previous[i]));
        }

        return results;
    }

    public void AddRange(IReadOnlyCollection<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (!Collection.TryAdd(entity.Identifier, entity))
                throw new EntityAlreadyExistsException<TIdentifier, TEntity>(entity.Identifier);
        }
    }

    public IReadOnlyCollection<TEntity> RemoveRange(
        IReadOnlyCollection<TIdentifier> identifiers,
        Func<TEntity, bool> filter
    )
    {
        foreach (var identifier in identifiers)
        {
            if (!Collection.TryGetValue(identifier, out var entity) || !filter(entity))
                throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);
        }

        var result = new List<TEntity>(identifiers.Count);

        foreach (var identifier in identifiers)
        {
            if (!Collection.TryRemove(identifier, out var entity))
                throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

            result.Add(entity);
        }

        return result;
    }

    public void Upsert(IReadOnlyCollection<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Collection.AddOrUpdate(entity.Identifier, entity, (_, _) => entity);
        }
    }
#endregion

#region Read
    public bool Exists(TIdentifier identifier)
    {
        return Collection.ContainsKey(identifier);
    }

    public TEntity? FindById(
        TIdentifier identifier,
        Func<TEntity, bool> filter
    )
    {
        if (!Collection.TryGetValue(identifier, out var entity))
            return null;

        return filter(entity) ? entity : null;
    }

    public TEntity? Find(
        Specification<TEntity> specification,
        Func<TEntity, bool> filter
    )
    {
        foreach (var pair in Collection)
        {
            var value = pair.Value;

            if (filter(value) && specification.IsSatisfiedBy(value))
                return value;
        }

        return null;
    }

    public IAsyncEnumerable<TEntity> FindMany(
        Specification<TEntity> specification,
        Func<TEntity, bool> filter
    )
    {
        var result = specification.Evaluate(ToFilteredValuesNonAlloc());

        return result.ToAsyncEnumerable();

        IEnumerable<TEntity> ToFilteredValuesNonAlloc()
        {
            foreach (var pair in Collection)
            {
                var value = pair.Value;

                if (filter(value))
                    yield return value;
            }
        }
    }

    public async IAsyncEnumerable<TEntity> FindManyById(
        IEnumerable<TIdentifier> identifiers,
        Func<TEntity, bool> filter
    )
    {
        foreach (var identifier in identifiers)
        {
            if (Collection.TryGetValue(identifier, out var entity) && filter(entity))
                yield return entity;
        }
    }
#endregion
}
