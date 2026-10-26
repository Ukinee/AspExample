using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;

namespace Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories.Transacted;

public enum InMemoryEntityState
{
    /// <summary>
    ///     The entity is not being tracked by the context.
    /// </summary>
    Detached = 0,

    /// <summary>
    ///     The entity is being tracked by the context and exists in the database. It has been marked
    ///     for deletion from the database.
    /// </summary>
    Deleted = 1,

    /// <summary>
    ///     The entity is being tracked by the context and exists in the database. Some or all of its
    ///     property values have been modified.
    /// </summary>
    Modified = 2,

    /// <summary>
    ///     The entity is being tracked by the context but does not yet exist in the database.
    /// </summary>
    Added = 3,
}

public record InMemoryEntry<TIdentifier, TEntity>(TIdentifier Identifier, TEntity? Entity, InMemoryEntityState EntityState, long OriginalVersion)
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull;

public class InMemoryChangeTracker<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

    private volatile bool _isFrozen = false;

    private Dictionary<TIdentifier, InMemoryEntry<TIdentifier, TEntity>> _entries = [];

    public void Freeze()
    {
        _isFrozen = true;
    }

    public List<InMemoryEntry<TIdentifier, TEntity>> GetExisting()
    {
        _locker.EnterReadLock();

        try
        {
            return _entries
                .Values.Where(entry => entry.Entity != null && entry.EntityState is not (InMemoryEntityState.Detached or InMemoryEntityState.Deleted))
                .ToList();
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    internal IEnumerable<InMemoryEntry<TIdentifier, TEntity>> GetAll()
    {
        return _entries.Values;
    }

    public InMemoryEntry<TIdentifier, TEntity> Entry(TIdentifier identifier)
    {
        _locker.EnterReadLock();

        try
        {
            return EntryCore(identifier);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    private InMemoryEntry<TIdentifier, TEntity> EntryCore(TIdentifier identifier) =>
        _entries.TryGetValue(identifier, out var entry)
            ? entry
            : new InMemoryEntry<TIdentifier, TEntity>(identifier, null, InMemoryEntityState.Detached, 0);

    public InMemoryEntry<TIdentifier, TEntity> Entry(TEntity entity)
    {
        _locker.EnterReadLock();

        try
        {
            if (_entries.TryGetValue(entity.Identifier, out var entry))
            {
                return entry;
            }

            return new InMemoryEntry<TIdentifier, TEntity>(entity.Identifier, entity, InMemoryEntityState.Detached, GetVersionOrZero(entity));
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public void AddRange(IReadOnlyCollection<TEntity> entities)
    {
        ThrowIfFrozen();

        _locker.EnterWriteLock();

        try
        {
            foreach (var entity in entities)
            {
                var inMemoryEntry = EntryCore(entity.Identifier);

                if (inMemoryEntry.EntityState is not (InMemoryEntityState.Detached or InMemoryEntityState.Deleted))
                {
                    throw new EntityAlreadyExistsException<TIdentifier, TEntity>(entity.Identifier);
                }
                
                if (inMemoryEntry.EntityState == InMemoryEntityState.Deleted)
                    throw new InvalidOperationException($"Cannot add entity with id {entity.Identifier} back.");

                var entry = new InMemoryEntry<TIdentifier, TEntity>(entity.Identifier, entity, InMemoryEntityState.Added, GetVersionOrZero(entity));

                _entries[entity.Identifier] = entry;
            }
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    public void RemoveRange(IEnumerable<TIdentifier> identifiers)
    {
        ThrowIfFrozen();

        _locker.EnterWriteLock();

        try
        {
            foreach (var identifier in identifiers)
            {
                _entries[identifier] = new InMemoryEntry<TIdentifier, TEntity>(identifier, null, InMemoryEntityState.Deleted, 0);
            }
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        ThrowIfFrozen();

        _locker.EnterWriteLock();

        try
        {
            foreach (var entity in entities)
            {
                if (_entries.TryGetValue(entity.Identifier, out var existingEntry) && existingEntry.EntityState is InMemoryEntityState.Deleted)
                {
                    throw new EntityNotFoundException<TIdentifier, TEntity>(entity.Identifier);
                }

                var newState = existingEntry?.EntityState == InMemoryEntityState.Added
                    ? InMemoryEntityState.Added
                    : InMemoryEntityState.Modified;

                _entries[entity.Identifier] = new InMemoryEntry<TIdentifier, TEntity>(entity.Identifier, entity, newState, GetVersionOrZero(entity));
            }
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    private long GetVersionOrZero(TEntity entity)
    {
        if (entity is not IEntityWithVersion<TEntity> versioned)
            return 0;

        return versioned.GlobalVersion;
    }

    private void ThrowIfFrozen()
    {
        if (_isFrozen)
            throw new InvalidOperationException("Cannot change tracker while it is frozen.");
    }
}
