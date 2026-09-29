using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Local.Repositories;

namespace Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;

public class InMemoryRepository<TIdentifier, TEntity> : IEditableTrackedRepository<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : IEntity<TIdentifier>
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private readonly Dictionary<TIdentifier, TEntity> _entities = [];

    public async Task<TEntity?> FindByIdAsync(TIdentifier identifier, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
    {
        var filterFunc = filter.Compile();

        _lock.EnterReadLock();

        try
        {
            return FindByIdCore(identifier, filterFunc);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<TEntity?> FindAsync(Specification<TEntity> specification, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
    {
        var filterFunc = filter.Compile();

        _lock.EnterReadLock();

        try
        {
            return specification.Evaluate(EnumerateOverValuesNoAlloc(filterFunc, cancellationToken)).FirstOrDefault();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IAsyncEnumerable<TEntity> FindManyAsync(Specification<TEntity> specification, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
    {
        var filterFunc = filter.Compile();

        _lock.EnterReadLock();

        try
        {
            return specification.Evaluate(EnumerateOverValuesNoAlloc(filterFunc, cancellationToken)).ToAsyncEnumerable();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async IAsyncEnumerable<TEntity> FindManyByIdAsync(
        IEnumerable<TIdentifier> identifiers,
        Expression<Func<TEntity, bool>> filter,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var filterFunc = filter.Compile();

        _lock.EnterReadLock();

        try
        {
            foreach (var identifier in identifiers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entity = _entities.GetValueOrDefault(identifier);

                if (entity != null && filterFunc.Invoke(entity))
                {
                    yield return entity;
                }
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task AddRange(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        _lock.EnterWriteLock();

        try
        {
            foreach (var entity in entities)
            {
                if (_entities.TryAdd(entity.Identifier, entity) == false)
                {
                    throw new EntityAlreadyExistsException<TIdentifier, TEntity>(entity.Identifier);
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task<UpdateResult<TEntity>> UpdateByIdAsync(
        TIdentifier identifier,
        Expression<Func<TEntity, bool>> filter,
        Func<TEntity, TEntity> update,
        CancellationToken cancellationToken
    )
    {
        var filterFunc = filter.Compile();

        _lock.EnterUpgradeableReadLock();

        try
        {
            var exiting = GetByIdCore(identifier, filterFunc);

            _lock.EnterWriteLock();

            try
            {
                var updated = update.Invoke(exiting);

                _entities[identifier] = updated;

                return new UpdateResult<TEntity>(updated, exiting);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public async Task<IReadOnlyList<UpdateResult<TEntity>>> UpdateManyByIdAsync(
        IReadOnlyCollection<TIdentifier> identifiers,
        Expression<Func<TEntity, bool>> filter,
        Func<TIdentifier, TEntity, TEntity> update,
        CancellationToken cancellationToken
    )
    {
        var result = new List<UpdateResult<TEntity>>(identifiers.Count);
        var filterFunc = filter.Compile();

        _lock.EnterWriteLock();

        try
        {
            foreach (var identifier in identifiers)
            {
                var exiting = GetByIdCore(identifier, filterFunc);
                var updated = update.Invoke(exiting.Identifier, exiting);
                _entities[identifier] = updated;

                result.Add(new UpdateResult<TEntity>(updated, exiting));
            }

            return result;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task<IReadOnlyCollection<TEntity>> RemoveRange(
        IReadOnlyCollection<TIdentifier> identifiers,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken
    )
    {
        var result = new List<TEntity>(identifiers.Count);
        var filterFunc = filter.Compile();

        _lock.EnterWriteLock();

        try
        {
            foreach (var identifier in identifiers)
            {
                var existing = GetByIdCore(identifier, filterFunc);
                
                _entities.Remove(identifier);
                
                result.Add(existing);
            }

            return result;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private IEnumerable<TEntity> EnumerateOverValuesNoAlloc(
        Func<TEntity, bool> filter,
        CancellationToken cancellationToken
    )
    {
        foreach ((_, var entity) in _entities)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (filter.Invoke(entity))
                yield return entity;
        }
    }

    private TEntity? FindByIdCore(TIdentifier identifier, Func<TEntity, bool> filter)
    {
        if (_entities.TryGetValue(identifier, out var entity) == false || filter.Invoke(entity) == false)
            return default;

        return entity;
    }

    private TEntity GetByIdCore(TIdentifier identifier, Func<TEntity, bool> filter)
    {
        var exiting = FindByIdCore(identifier, filter);

        return exiting ?? throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);
    }
}
