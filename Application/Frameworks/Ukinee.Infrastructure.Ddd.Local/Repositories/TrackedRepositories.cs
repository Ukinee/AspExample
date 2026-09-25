using System.Linq.Expressions;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;

namespace Ukinee.Infrastructure.Ddd.Local.Repositories;


public interface ITrackedRepository<in TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task<TEntity?> FindByIdAsync(TIdentifier identifier, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);
    public Task<TEntity?> FindAsync(Specification<TEntity> specification, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);
    public IAsyncEnumerable<TEntity> FindManyAsync(Specification<TEntity> specification, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);
    public IAsyncEnumerable<TEntity> FindManyByIdAsync(IEnumerable<TIdentifier> identifiers, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);
}

public interface IEditableTrackedRepository<TIdentifier, TEntity> : ITrackedRepository<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task AddRange(IReadOnlyCollection<TEntity> entities);

    Task<UpdateResult<TEntity>> UpdateByIdAsync(
        TIdentifier identifier,
        UpdateLock mode,
        Expression<Func<TEntity, bool>> filter,
        Func<TEntity, TEntity> update,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<UpdateResult<TEntity>>> UpdateManyByIdAsync(
        IReadOnlyCollection<TIdentifier> identifiers,
        UpdateLock mode,
        Expression<Func<TEntity, bool>> filter,
        Func<TIdentifier, TEntity, TEntity> update,
        CancellationToken cancellationToken
    );

    public Task<IReadOnlyCollection<TEntity>> RemoveRange(IEnumerable<TIdentifier> identifier, Expression<Func<TEntity, bool>> filter);
}
