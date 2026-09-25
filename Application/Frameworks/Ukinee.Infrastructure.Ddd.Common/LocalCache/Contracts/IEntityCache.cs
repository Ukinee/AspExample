using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;

public enum CacheLookupStatus
{
    Unknown,
    Hit,
    KnownMissing,
}

public readonly record struct CacheLookupResult<TEntity>(CacheLookupStatus Status, TEntity? Entity)
where TEntity : class
{
    public bool IsHit => Status == CacheLookupStatus.Hit;
    public bool IsKnownMissing => Status == CacheLookupStatus.KnownMissing;
    public bool IsUnknown => Status == CacheLookupStatus.Unknown;
}

public interface IEntityCache<in TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    CacheLookupResult<TEntity> Lookup(TIdentifier id);

    void Upsert(TEntity entity);
    void Upsert(IReadOnlyCollection<TEntity> entities);

    void MarkNotExists(TIdentifier id);
    void MarkNotExists(IReadOnlyCollection<TIdentifier> ids);

    void Invalidate(TIdentifier id);
    void Invalidate(IEnumerable<TIdentifier> ids);

    void CleanupExpired();
    void Clear();
}
