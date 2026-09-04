using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Extensions;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

public class TrackedDeltaEntityUpdater<TIdentifier, TEntity>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IIdentifierEntityAccessValidator<TIdentifier, TEntity> identifierEntityAccessValidator,
    IPublisher publisher
) : IDeltaEntityUpdater<TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity> Update(UserContext userContext, TEntity entity, UpdateLock mode, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken)
    {
        identifierEntityAccessValidator.EnsureHasAccess(userContext, entity.Identifier);

        var result = await repository.UpdateByIdAsync(entity.Identifier, mode, updateFactory, cancellationToken);

        await publisher.PublishUpdatedEvent<TIdentifier, TEntity>(userContext, result, mode, updateFactory);

        return result;
    }
}

public class TrackedPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IEntityUpdateFactory<TUpdatePayload, TEntity> updateFactory,
    IIdentifierEntityAccessValidator<TIdentifier, TEntity> identifierEntityAccessValidator,
    IPublisher publisher
) : IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity> UpdateAsync(UserContext userContext, TIdentifier identifier, TUpdatePayload payload, CancellationToken cancellationToken)
    {
        identifierEntityAccessValidator.EnsureHasAccess(userContext, identifier);

        var result = await repository.UpdateByIdAsync(identifier, UpdateLock.Delta, old => updateFactory.Update(userContext, old, payload), cancellationToken);

        await publisher.PublishUpdatedEvent<TIdentifier, TEntity>(userContext, result, UpdateLock.Delta, old => updateFactory.Update(userContext, old, payload));

        return result;
    }

    public async Task<IReadOnlyCollection<TEntity>> UpdateAsync(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TUpdatePayload>> payloads, CancellationToken cancellationToken)
    {
        var payloadsMap = payloads.ToDictionary(x => x.Identifier, x => x.Payload);

        identifierEntityAccessValidator.EnsureHasAccess(userContext, payloadsMap.Keys);

        var result = new List<TEntity>();
        var updates = new List<UpdateInfo<TEntity>>();

        foreach (var identifier in payloadsMap.Keys)
        {
            var payload = payloadsMap[identifier];

            Func<TEntity, TEntity> factory = old => updateFactory.Update(userContext, old, payload);

            var updatedEntity = await repository.UpdateByIdAsync(identifier, UpdateLock.Delta, factory, cancellationToken);
            var update = new UpdateInfo<TEntity>(updatedEntity, UpdateLock.Delta, factory);

            result.Add(updatedEntity);
            updates.Add(update);
        }

        await publisher.PublishUpdatedEvent<TIdentifier, TEntity>(userContext, updates);

        return result;
    }
}
