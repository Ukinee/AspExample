using System.Linq.Expressions;
using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;

namespace Ukinee.Infrastructure.Ddd.Local.Repositories;

public abstract class RepositoryBase<TIdentifier, TEntity> : IEditableTrackedRepository<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
#region Edit
    public async Task AddRange(IReadOnlyCollection<TEntity> entities)
    {
        await AddRangeInternal(entities);
        await Task.WhenAll(entities.Select(OnAfterAdd));
    }

    public abstract Task<UpdateResult<TEntity>> UpdateByIdAsync(
        TIdentifier identifier,
        UpdateLock mode,
        Expression<Func<TEntity, bool>> filter,
        Func<TEntity, TEntity> update,
        CancellationToken cancellationToken
    );

    public abstract Task<IReadOnlyList<UpdateResult<TEntity>>> UpdateManyByIdAsync(
        IReadOnlyCollection<TIdentifier> identifiers,
        UpdateLock mode,
        Expression<Func<TEntity, bool>> filter,
        Func<TIdentifier, TEntity, TEntity> update,
        CancellationToken cancellationToken
    );

    public async Task<IReadOnlyCollection<TEntity>> RemoveRange(IEnumerable<TIdentifier> identifiers, Expression<Func<TEntity, bool>> filter)
    {
        var result = await RemoveRangeInternal(identifiers, filter);
        await Task.WhenAll(result.Select(OnAfterRemoved));

        return result;
    }

    protected abstract Task AddRangeInternal(IReadOnlyCollection<TEntity> entities);
    protected abstract Task<IReadOnlyCollection<TEntity>> RemoveRangeInternal(IEnumerable<TIdentifier> identifiers, Expression<Func<TEntity, bool>> filter);
#endregion

#region Read
    public abstract Task<TEntity?> FindByIdAsync(TIdentifier identifier, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);

    public abstract IAsyncEnumerable<TEntity> FindManyByIdAsync(
        IEnumerable<TIdentifier> identifiers,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken
    );

    public abstract IAsyncEnumerable<TEntity> FindManyAsync(
        Specification<TEntity> specification,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken
    );

    public abstract Task<TEntity?> FindAsync(Specification<TEntity> specification, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);
#endregion

#region Reactions
    protected virtual Task OnAfterAdd(TEntity entity) =>
        Task.CompletedTask;

    protected virtual Task OnAfterRemoved(TEntity entity) =>
        Task.CompletedTask;
#endregion
}
