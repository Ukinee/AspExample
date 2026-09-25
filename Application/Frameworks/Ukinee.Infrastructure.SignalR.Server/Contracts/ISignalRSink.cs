using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.SignalR.Server.Contracts;

public interface ISignalRSink<TIdentifier, in TEntity>
where TEntity : IEntity<TIdentifier>
{
    public Task NotifyCreation(IReadOnlyCollection<TEntity> entities);
    public Task NotifyUpdate(IReadOnlyCollection<TEntity> entities);
    public Task NotifyRemoval(IReadOnlyCollection<TIdentifier> identifiers);
}
