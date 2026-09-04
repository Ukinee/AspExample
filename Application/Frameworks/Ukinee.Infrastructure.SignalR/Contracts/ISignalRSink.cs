using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.SignalR.Contracts;

public interface ISignalRSink<in TEntity>
where TEntity : IEntity
{
    public Task NotifyCreation(IReadOnlyCollection<TEntity> entities);
    public Task NotifyUpdate(IReadOnlyCollection<TEntity> entities);
    public Task NotifyRemoval(IReadOnlyCollection<TEntity> entities);
}
