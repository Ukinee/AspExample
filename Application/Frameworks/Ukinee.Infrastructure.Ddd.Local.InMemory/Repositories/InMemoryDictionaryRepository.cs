using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Local.Repositories;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;

public class InMemoryDictionaryRepository<TIdentifier, TEntity> : RepositoryBase<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    protected readonly ConcurrentDictionary<TIdentifier, TEntity> Collection = new();

#region Edit
    public override Task<TEntity> UpdateByIdAsync(TIdentifier identifier, UpdateLock mode, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken)
    {
        var updatedEntity = Collection.AddOrUpdate(
            identifier,
            id => throw new EntityNotFoundException<TIdentifier, TEntity>(id),
            (_, oldEntity) => updateFactory(oldEntity)
        );

        return Task.FromResult(updatedEntity);
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

    protected override Task<IReadOnlyCollection<TEntity>> RemoveRangeInternal(IEnumerable<TIdentifier> identifiers)
    {
        var result = new List<TEntity>();

        foreach (var identifier in identifiers)
        {
            if (!Collection.TryRemove(identifier, out var entity))
                throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

            result.Add(entity);
        }

        return Task.FromResult<IReadOnlyCollection<TEntity>>(result);
    }
#endregion

#region Read
    public override Task<TEntity?> FindByIdAsync(TIdentifier identifier, CancellationToken cancellationToken)
    {
        Collection.TryGetValue(identifier, out var entity);

        return Task.FromResult(entity);
    }

    public override Task<TEntity?> FindAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        foreach (var pair in Collection)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (specification.IsSatisfiedBy(pair.Value))
            {
                return Task.FromResult<TEntity?>(pair.Value);
            }
        }

        return Task.FromResult<TEntity?>(null);
    }

    public override IAsyncEnumerable<TEntity> FindManyAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var result = specification.Evaluate(ToValuesNonAlloc(cancellationToken));

        return result.ToAsyncEnumerable();

        IEnumerable<TEntity> ToValuesNonAlloc(CancellationToken token)
        {
            foreach (var pair in Collection)
            {
                if (token.IsCancellationRequested)
                    yield break;

                yield return pair.Value;
            }
        }
    }

    public override async IAsyncEnumerable<TEntity> FindManyByIdAsync(IEnumerable<TIdentifier> identifiers, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var identifier in identifiers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Collection.TryGetValue(identifier, out var entity))
            {
                yield return entity;
            }
        }
    }
#endregion
}
