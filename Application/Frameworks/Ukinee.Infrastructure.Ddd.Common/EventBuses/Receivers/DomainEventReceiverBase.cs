using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;

namespace Ukinee.Infrastructure.Ddd.Common.EventBuses.Receivers;

public abstract class DomainEventReceiverBase<TIdentifier, TEntity> :
    INotificationHandler<CreatedDomainEvent<TIdentifier, TEntity>>,
    INotificationHandler<UpdatedDomainEvent<TIdentifier, TEntity>>,
    INotificationHandler<DeletedDomainEvent<TIdentifier, TEntity>>
where TEntity : IEntity<TIdentifier>
{
    public async Task Handle(CreatedDomainEvent<TIdentifier, TEntity> notification, CancellationToken cancellationToken)
    {
        await OnCreated(notification.Entities, cancellationToken);
    }

    public async Task Handle(UpdatedDomainEvent<TIdentifier, TEntity> notification, CancellationToken cancellationToken)
    {
        await OnUpdated(notification.Updates, cancellationToken);
    }

    public async Task Handle(DeletedDomainEvent<TIdentifier, TEntity> notification, CancellationToken cancellationToken)
    {
        await OnRemoved(notification.Identifiers, cancellationToken);
    }

    protected abstract Task OnCreated(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken);
    protected abstract Task OnUpdated(IReadOnlyCollection<UpdateResult<TEntity>> updateInfos, CancellationToken cancellationToken);
    protected abstract Task OnRemoved(IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken);
}

public abstract class ClientDomainEventReceiverBase<TIdentifier, TEntity> :
    INotificationHandler<CreatedDomainEvent<TIdentifier, TEntity>>,
    INotificationHandler<UpdatedDomainEvent<TIdentifier, TEntity>>,
    INotificationHandler<DeletedClientDomainEvent<TIdentifier, TEntity>>,
    INotificationHandler<UpsertClientDomainEvent<TIdentifier, TEntity>>
where TEntity : IEntity<TIdentifier>
{
    public async Task Handle(CreatedDomainEvent<TIdentifier, TEntity> notification, CancellationToken cancellationToken)
    {
        await OnCreated(notification.Entities, cancellationToken);
    }

    public async Task Handle(UpdatedDomainEvent<TIdentifier, TEntity> notification, CancellationToken cancellationToken)
    {
        await OnUpdated(notification.Updates, cancellationToken);
    }

    public async Task Handle(DeletedClientDomainEvent<TIdentifier, TEntity> notification, CancellationToken cancellationToken)
    {
        await OnRemoved(notification.DeletedIdentifiers, cancellationToken);
    }

    public async Task Handle(UpsertClientDomainEvent<TIdentifier, TEntity> notification, CancellationToken cancellationToken)
    {
        await OnUpsert(notification.UpsertEntities, cancellationToken);
    }

    protected abstract Task OnCreated(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken);
    protected abstract Task OnUpdated(IReadOnlyCollection<UpdateResult<TEntity>> updates, CancellationToken cancellationToken);
    protected abstract Task OnRemoved(IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken);
    protected abstract Task OnUpsert(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken);
}
