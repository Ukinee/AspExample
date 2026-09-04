using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class UpdateEntityUseCase<TIdentifier, TEntity>(IDeltaEntityUpdater<TEntity> updater) : IUpdateEntityUseCase<TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task<TEntity> Execute(UserContext userContext, TEntity entity, UpdateLock mode, Func<TEntity, TEntity> updateFactory) =>
        updater.Update(userContext, entity, mode, updateFactory, CancellationToken.None);
}

public class UpdateEntityUseCase<TIdentifier, TPayload, TEntity>(IPayloadEntityUpdater<TIdentifier, TPayload, TEntity> updater) :
    IUpdateEntityUseCase<TPayload, TEntity>,
    IUpdateEntityUseCase<TIdentifier, TPayload, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task<TEntity> Execute(UserContext userContext, TEntity referenceEntity, TPayload payload) =>
        updater.UpdateAsync(userContext, referenceEntity.Identifier, payload, CancellationToken.None);

    public Task<TEntity> Execute(UserContext userContext, TIdentifier identfier, TPayload payload) =>
        updater.UpdateAsync(userContext, identfier, payload, CancellationToken.None);

    public Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> payloads) =>
        updater.UpdateAsync(userContext, payloads, CancellationToken.None);
}
