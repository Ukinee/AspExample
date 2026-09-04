using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;

namespace Ukinee.Infrastructure.Ddd.Common.EventBuses.Receivers;

public abstract class DomainEventReceiverBase<TIdentifier, TEntity> : INotificationHandler<DomainEvent<TIdentifier, TEntity>>
where TEntity : IEntity<TIdentifier>
{
    public async Task Handle(DomainEvent<TIdentifier, TEntity> notification, CancellationToken cancellationToken)
    {
        var task = notification.Action switch
        {
            DomainEventAction.Created => OnCreated(notification.CreatedEntities!, cancellationToken),
            DomainEventAction.Updated => OnUpdated(notification.UpdateInfos!, cancellationToken),
            DomainEventAction.Removed => OnRemoved(notification.DeletedEntities!, cancellationToken),
        };

        if (task != null)
            await task;
    }

    protected abstract Task OnCreated(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken);
    protected abstract Task OnUpdated(IReadOnlyCollection<UpdateInfo<TEntity>> updates, CancellationToken cancellationToken);
    protected abstract Task OnRemoved(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken);

}
