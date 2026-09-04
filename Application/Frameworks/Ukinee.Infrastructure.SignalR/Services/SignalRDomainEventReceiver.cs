using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Receivers;
using Ukinee.Infrastructure.SignalR.Contracts;

namespace Ukinee.Infrastructure.SignalR.Services;

public class SignalRDomainEventReceiver<TIdentifier, TEntity>(ISignalRSink<TEntity> sink) : DomainEventReceiverBase<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    protected override Task OnCreated(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken) =>
        sink.NotifyCreation(entities);

    protected override Task OnUpdated(IReadOnlyCollection<UpdateInfo<TEntity>> updates, CancellationToken cancellationToken) =>
        sink.NotifyUpdate(updates.Select(d => d.UpdateResult).ToList());

    protected override Task OnRemoved(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken) =>
        sink.NotifyRemoval(entities);
}
