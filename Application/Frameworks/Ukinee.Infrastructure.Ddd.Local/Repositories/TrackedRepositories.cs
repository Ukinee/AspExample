using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;

namespace Ukinee.Infrastructure.Ddd.Local.Repositories;

public interface ITrackedRepository<in TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task<TEntity?> FindByIdAsync(TIdentifier identifier, CancellationToken cancellationToken);

    public IAsyncEnumerable<TEntity> FindManyByIdAsync(IEnumerable<TIdentifier> identifiers, CancellationToken cancellationToken);
    public IAsyncEnumerable<TEntity> FindManyAsync(Specification<TEntity> specification, CancellationToken cancellationToken);

    public Task<TEntity?> FindAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
}

public interface IEditableTrackedRepository<in TIdentifier, TEntity> : ITrackedRepository<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task AddRange(IReadOnlyCollection<TEntity> entities);

    public Task<TEntity> UpdateByIdAsync(TIdentifier identifier, UpdateLock mode, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken);

    public Task<IReadOnlyCollection<TEntity>> RemoveRange(IEnumerable<TIdentifier> identifier);
}
