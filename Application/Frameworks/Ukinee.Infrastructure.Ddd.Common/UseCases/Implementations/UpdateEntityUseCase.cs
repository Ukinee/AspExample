using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class UpdateEntityUseCase<TIdentifier, TPayload, TEntity>(IPayloadEntityUpdater<TIdentifier, TPayload, TEntity> updater) :
    IUpdateEntityUseCase<TPayload, TEntity>,
    IUpdateEntityUseCase<TIdentifier, TPayload, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task<TEntity> Execute(UserContext userContext, TEntity referenceEntity, TPayload payload, CancellationToken cancellationToken) =>
        updater.UpdateAsync(userContext, referenceEntity.Identifier, payload, cancellationToken);

    public Task<TEntity> Execute(UserContext userContext, TIdentifier identfier, TPayload payload, CancellationToken cancellationToken) =>
        updater.UpdateAsync(userContext, identfier, payload, cancellationToken);

    public Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> payloads, CancellationToken cancellationToken) =>
        updater.UpdateAsync(userContext, payloads, cancellationToken);
}
