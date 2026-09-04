using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;

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

    public async Task<IReadOnlyCollection<TEntity>> RemoveRange(IEnumerable<TIdentifier> identifiers)
    {
        var result = await RemoveRangeInternal(identifiers);
        await Task.WhenAll(result.Select(OnAfterRemoved));

        return result;
    }

    public abstract Task<TEntity> UpdateByIdAsync(TIdentifier identifier, UpdateLock mode, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken);

    protected abstract Task AddRangeInternal(IReadOnlyCollection<TEntity> entities);
    protected abstract Task<IReadOnlyCollection<TEntity>> RemoveRangeInternal(IEnumerable<TIdentifier> identifiers);
#endregion

#region Read
    public abstract Task<TEntity?> FindByIdAsync(TIdentifier identifier, CancellationToken cancellationToken);
    public abstract IAsyncEnumerable<TEntity> FindManyByIdAsync(IEnumerable<TIdentifier> identifiers, CancellationToken cancellationToken);
    public abstract IAsyncEnumerable<TEntity> FindManyAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
    public abstract Task<TEntity?> FindAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
#endregion

#region Reactions
    protected virtual Task OnAfterAdd(TEntity entity) =>
        Task.CompletedTask;

    protected virtual Task OnAfterRemoved(TEntity entity) =>
        Task.CompletedTask;
#endregion
}
