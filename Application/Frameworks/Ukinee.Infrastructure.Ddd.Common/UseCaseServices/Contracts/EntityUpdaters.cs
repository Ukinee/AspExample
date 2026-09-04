using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;

public interface IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public Task<TEntity> UpdateAsync(UserContext userContext, TIdentifier identifier, TUpdatePayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> UpdateAsync(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TUpdatePayload>> payloads, CancellationToken cancellationToken);
}

public interface IDeltaEntityUpdater<TEntity>
where TEntity : class, IEntity
{
    public Task<TEntity> Update(UserContext userContext, TEntity entity, UpdateLock mode, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken);
}
