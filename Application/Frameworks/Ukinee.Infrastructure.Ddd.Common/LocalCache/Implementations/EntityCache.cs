using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Options;

namespace Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;

public sealed class EntityCache<TIdentifier, TEntity>(IOptions<CachingOptions> options, TimeProvider timeProvider)
    : IEntityCache<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
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
        var expiresAt = GetExpiration();
        _entries[entity.Identifier] = new Entry(entity, expiresAt);
    }

    public void Upsert(IReadOnlyCollection<TEntity> entities)
    {
        var expiresAt = GetExpiration();

        foreach (var entity in entities)
        {
            _entries[entity.Identifier] = new Entry(entity, expiresAt);
        }
    }

    public void MarkNotExists(TIdentifier id)
    {
        _entries[id] = new Entry(null, GetExpiration());
    }

    public void MarkNotExists(IReadOnlyCollection<TIdentifier> ids)
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
        {
            if (entry.ExpiresAt <= now)
                _entries.TryRemove(key, out _);
        }
    }

    public void Clear()
    {
        _entries.Clear();
    }
    
    private DateTimeOffset GetExpiration() =>
        timeProvider.GetUtcNow() + options.Value.CacheDuration;

    private readonly record struct Entry(TEntity? Entity, DateTimeOffset ExpiresAt);
}
