using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Receivers;
using Ukinee.Infrastructure.SignalR.Server.Contracts;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public class SignalRLocalDomainEventReceiver<TIdentifier, TEntity>(ISignalRSink<TIdentifier, TEntity> sink) : LocalDomainEventReceiverBase<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    protected override Task OnCreated(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken) =>
        sink.NotifyCreation(entities);

    protected override Task OnUpdated(IReadOnlyCollection<UpdateResult<TEntity>> updates, CancellationToken cancellationToken) =>
        sink.NotifyUpdate(updates.Select(d => d.Updated).ToList());

    protected override Task OnRemoved(IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken) =>
        sink.NotifyRemoval(entities);
}
