using FluentValidation;
using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

public sealed class TrackedDeltaEntityUpdater<TIdentifier, TEntity>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity> accessProvider,
    IPublisher publisher
) : IDeltaEntityUpdater<TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity> Update(
        UserContext userContext,
        TEntity entity,
        Func<TEntity, TEntity> updateFactory,
        CancellationToken cancellationToken
    )
    {
        var filter = await accessProvider.GetUpdateExpression(userContext);

        var result = await repository.UpdateByIdAsync(
            entity.Identifier,
            filter,
            updateFactory,
            cancellationToken
        );

        if (result is null)
            throw new EntityNotFoundException<TIdentifier, TEntity>(entity.Identifier);

        await publisher.PublishUpdatedEvent<TIdentifier, TEntity>(userContext, [result], cancellationToken);

        return result.Updated;
    }
}

public class TrackedPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IValidator<IEnumerable<TUpdatePayload>> validationService,
    IEntityUpdateFactory<TUpdatePayload, TEntity> updateFactory,
    IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity> accessProvider,
    IPublisher publisher
) : IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity> UpdateAsync(
        UserContext userContext,
        TIdentifier identifier,
        TUpdatePayload payload,
        CancellationToken cancellationToken
    )
    {
        await validationService.ValidateAndThrowAsync([payload], cancellationToken);

        var filter = await accessProvider.GetUpdateExpression(userContext);

        var result = await repository.UpdateByIdAsync(
            identifier,
            filter,
            old => updateFactory.Update(userContext, old, payload),
            cancellationToken
        );

        if (result is null)
            throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

        await publisher.PublishUpdatedEvent<TIdentifier, TEntity>(userContext, [result], cancellationToken);

        return result.Updated;
    }

    public async Task<IReadOnlyCollection<TEntity>> UpdateAsync(
        UserContext userContext,
        IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TUpdatePayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        if (payloads.Count == 0)
            return Array.Empty<TEntity>();

        await validationService.ValidateAndThrowAsync(payloads.Select(d => d.Payload), cancellationToken);

        var payloadById = payloads.ToDictionary(p => p.Identifier, p => p.Payload);
        var identifiers = payloadById.Keys.ToArray();

        var filter = await accessProvider.GetUpdateExpression(userContext);

        var results = await repository.UpdateManyByIdAsync(
            identifiers,
            filter,
            (id, old) => updateFactory.Update(userContext, old, payloadById[id]),
            cancellationToken
        );

        if (results.Count != identifiers.Length)
        {
            var missing = identifiers
                .Except(results.Select(r => r.Updated.Identifier))
                .ToArray();

            throw new EntityNotFoundException<TIdentifier, TEntity>(missing);
        }

        await publisher.PublishUpdatedEvent<TIdentifier, TEntity>(userContext, results, cancellationToken);

        return results.Select(r => r.Updated).ToArray();
    }
}
