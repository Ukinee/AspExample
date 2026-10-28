using System.Collections.Concurrent;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;

namespace Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories.Transacted;

public class InMemoryStore<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public readonly record struct StoredEntry(TEntity Entity, long Version);

    public readonly ConcurrentDictionary<TIdentifier, StoredEntry> Collection = new();

    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private long _nextVersion;

    public TEntity? FindById(TIdentifier identifier, Func<TEntity, bool> predicate)
    {
        return Collection.TryGetValue(identifier, out var e) && predicate(e.Entity)
            ? VersionPolicy<TEntity>.Materialize(e.Entity, e.Version)
            : null;
    }

    public IAsyncEnumerable<TEntity> FindMany(Specification<TEntity> specification, Func<TEntity, bool> filter)
    {
        var result = specification.Evaluate(ToFilteredValuesNonAlloc());

        return result.ToAsyncEnumerable();

        IEnumerable<TEntity> ToFilteredValuesNonAlloc()
        {
            foreach (var pair in Collection)
            {
                var value = pair.Value;

                if (filter(value.Entity))
                    yield return VersionPolicy<TEntity>.Materialize(value.Entity, value.Version);
            }
        }
    }

    public bool Exists(TIdentifier identifier) =>
        Collection.ContainsKey(identifier);

    public async Task WriteAsync(Action<WriteSession> action, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken);

        try
        {
            action(new WriteSession(this));
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private long GetVersion(TIdentifier identifier)
    {
        if (!Collection.TryGetValue(identifier, out var entry))
            throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

        return VersionPolicy<TEntity>.GetVersionOrDefault(entry.Entity);
    }

    public sealed class WriteSession
    {
        private readonly InMemoryStore<TIdentifier, TEntity> _store;

        internal WriteSession(InMemoryStore<TIdentifier, TEntity> store) =>
            _store = store;

        public long GetVersion(TIdentifier identifier) =>
            _store.GetVersion(identifier);

        public void UpsertNew(TEntity entity)
        {
            var identifier = entity.Identifier;

            if (_store.Collection.ContainsKey(identifier))
                throw new EntityAlreadyExistsException<TIdentifier, TEntity>(identifier);

            var version = Interlocked.Increment(ref _store._nextVersion);
            _store.Collection[identifier] = new StoredEntry(VersionPolicy<TEntity>.Materialize(entity, version), version);
        }

        public void UpsertIfVersion(TEntity entity, long expectedVersion)
        {
            var identifier = entity.Identifier;

            var currentVersion = GetVersion(identifier);

            if (currentVersion != expectedVersion)
                throw new ConcurrencyException(identifier, expectedVersion, currentVersion);

            var version = Interlocked.Increment(ref _store._nextVersion);
            _store.Collection[identifier] = new StoredEntry(VersionPolicy<TEntity>.Materialize(entity, version), version);
        }

        public void RemoveIfVersion(TIdentifier identifier, long expectedVersion)
        {
            var currentVersion = GetVersion(identifier);

            if (currentVersion != expectedVersion)
                throw new ConcurrencyException(identifier, expectedVersion, currentVersion);

            _store.Collection.Remove(identifier, out _);
        }

        internal void ForceUpsert(TEntity entity, long version) =>
            _store.Collection[entity.Identifier] = new StoredEntry(VersionPolicy<TEntity>.Materialize(entity, version), version);

        internal void ForceRemove(TIdentifier id) =>
            _store.Collection.Remove(id, out _);

        public bool Exists(TIdentifier identifier) =>
            _store.Collection.TryGetValue(identifier, out var e);

        public TEntity FindById(TIdentifier identifier)
        {
            _store.Collection.TryGetValue(identifier, out var e);

            return e.Entity;
        }

        public long GetExistingVersionOrZero(TIdentifier identifier)
        {
            if (_store.Collection.TryGetValue(identifier, out var e))
                return GetVersion(identifier);

            return 0;
        }
    }
}

public class ConcurrencyException : Exception
{
    public long ExpectedVersion { get; }
    public long CurrentVersion { get; }

    public ConcurrencyException(object identifier, long expectedVersion, long currentVersion) : base(
        $"Entity with identifier {identifier} was changed. Expected version {expectedVersion}, current version {currentVersion}"
    )
    {
        ExpectedVersion = expectedVersion;
        CurrentVersion = currentVersion;
    }
}
