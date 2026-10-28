using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Options;

namespace Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;

public class VersionedEntityCache<TIdentifier, TEntity>(IOptions<CachingOptions> options, TimeProvider timeProvider) : IEntityCache<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>, IEntityWithVersion<TEntity>
where TIdentifier : struct
{
    private readonly record struct Entry(TEntity? Entity, DateTimeOffset ExpiresAt);

    private readonly ConcurrentDictionary<TIdentifier, Entry> _entries = new ConcurrentDictionary<TIdentifier, Entry>();

    public CacheLookupResult<TEntity> Lookup(TIdentifier id)
    {
        if (!_entries.TryGetValue(id, out var entry))
            return new CacheLookupResult<TEntity>(CacheLookupStatus.Unknown, null);

        if (entry.ExpiresAt <= timeProvider.GetUtcNow())
        {
            _entries.TryRemove(new KeyValuePair<TIdentifier, Entry>(id, entry));

            return new CacheLookupResult<TEntity>(CacheLookupStatus.Unknown, null);
        }

        return entry.Entity is not null
            ? new CacheLookupResult<TEntity>(CacheLookupStatus.Hit, entry.Entity)
            : new CacheLookupResult<TEntity>(CacheLookupStatus.KnownMissing, null);
    }

    public void Upsert(TEntity entity)
    {
        if (ShouldUpdateEntry(entity))
            SetEntry(entity.Identifier, entity);
    }

    public void Upsert(IEnumerable<TEntity> entities)
    {
        var expiresAt = GetExpiration();

        foreach (var entity in entities)
            if (ShouldUpdateEntry(entity))
                SetEntry(entity.Identifier, entity, expiresAt);
    }

    public void MarkNotExists(TIdentifier id)
    {
        _entries[id] = new Entry(null, GetExpiration());
    }

    public void MarkNotExists(IEnumerable<TIdentifier> ids)
    {
        var expiresAt = GetExpiration();

        foreach (var id in ids)
            _entries[id] = new Entry(null, expiresAt);
    }

    public void Invalidate(IEnumerable<TIdentifier> ids)
    {
        foreach (var id in ids)
            _entries.TryRemove(id, out _);
    }

    public void Invalidate(TIdentifier id)
    {
        _entries.TryRemove(id, out _);
    }

    public void CleanupExpired()
    {
        var now = timeProvider.GetUtcNow();

        foreach ((var key, var entry) in _entries)
            if (entry.ExpiresAt <= now)
                _entries.TryRemove(key, out _);
    }

    public void Clear() =>
        _entries.Clear();

    private DateTimeOffset GetExpiration() =>
        timeProvider.GetUtcNow() + options.Value.CacheDuration;

    private bool ShouldUpdateEntry(TEntity entity)
    {
        if (!_entries.TryGetValue(entity.Identifier, out var entry))
            return true;

        if (entry.Entity == null)
            return true;

        return entry.Entity.GlobalVersion < entity.GlobalVersion;
    }

    private void SetEntry(TIdentifier identifier, TEntity? entity, DateTimeOffset expiresAt) =>
        _entries[identifier] = new Entry(entity, expiresAt);

    private void SetEntry(TIdentifier identifier, TEntity? entity)
    {
        var expiresAt = GetExpiration();
        SetEntry(identifier, entity, expiresAt);
    }
}
