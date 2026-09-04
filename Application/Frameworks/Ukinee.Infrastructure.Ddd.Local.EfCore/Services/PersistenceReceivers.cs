using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Receivers;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Repositories;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

public class DeletableDatabasePersistenceReceiver<TIdentifier, TEntity, TTag>(DbService<TIdentifier, TEntity, TTag> service) : DomainEventReceiverBase<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>, ISpecificationForSoftDelete<TEntity>
where TIdentifier : struct, IEquatable<TIdentifier>
{
    protected override async Task OnCreated(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        await service.AddRange(entities);
    }

    protected override async Task OnUpdated(IReadOnlyCollection<UpdateInfo<TEntity>> updateInfos, CancellationToken cancellationToken)
    {
        foreach (var updateInfo in updateInfos)
        {
            await service.UpdateByIdAsync(updateInfo.UpdateResult.Identifier, updateInfo.Mode, updateInfo.UpdateFactory, cancellationToken);
        }
    }

    protected override async Task OnRemoved(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            await service.UpdateByIdAsync(entity.Identifier, UpdateLock.Delta, old => old.Delete(entity.DeletedAt), cancellationToken);
        }
    }
}

public class HonestDatabasePersistenceReceiver<TIdentifier, TEntity, TTag>(DbService<TIdentifier, TEntity, TTag> service) : DomainEventReceiverBase<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>
{
    protected override async Task OnCreated(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        await service.AddRange(entities);
    }

    protected override async Task OnUpdated(IReadOnlyCollection<UpdateInfo<TEntity>> updateInfos, CancellationToken cancellationToken)
    {
        foreach (var updateInfo in updateInfos)
        {
            await service.UpdateByIdAsync(updateInfo.UpdateResult.Identifier, updateInfo.Mode, updateInfo.UpdateFactory, cancellationToken);
        }
    }

    protected override async Task OnRemoved(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        await service.RemoveRange(entities.Select(e => e.Identifier));
    }
}
