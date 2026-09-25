using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

public class HonestTrackedRemover<TIdentifier, TEntity>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity> accessProvider,
    IPublisher publisher
) : IEntityRemover<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        if (entities.Count == 0)
            return Task.CompletedTask;

        var identifiers = entities
            .Select(e => e.Identifier)
            .Distinct()
            .ToArray();

        return RemoveAsync(userContext, identifiers, cancellationToken);
    }

    public async Task RemoveAsync(
        UserContext userContext,
        IReadOnlyCollection<TIdentifier> identifiers,
        CancellationToken cancellationToken
    )
    {
        if (identifiers.Count == 0)
            return;

        var access = await accessProvider.GetDeleteExpression(userContext);

        var removed = await repository.RemoveRange(identifiers, access);

        if (removed.Count != identifiers.Count)
            throw new EntityNotFoundException<TIdentifier, TEntity>(identifiers);

        await publisher.PublishRemovedEvent<TIdentifier, TEntity>(userContext, identifiers, cancellationToken);
    }
}

public sealed class DeletableTrackedRemover<TIdentifier, TEntity>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity> accessValidator,
    IPublisher publisher,
    TimeProvider timeProvider
) : IEntityRemover<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>, IEntityWithSoftDelete<TEntity>
{
    public Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        if (entities.Count == 0)
            return Task.CompletedTask;

        var identifiers = entities
            .Select(e => e.Identifier)
            .Distinct()
            .ToArray();

        return RemoveAsync(userContext, identifiers, cancellationToken);
    }

    public async Task RemoveAsync(
        UserContext userContext,
        IReadOnlyCollection<TIdentifier> identifiers,
        CancellationToken cancellationToken
    )
    {
        if (identifiers.Count == 0)
            return;

        var now = timeProvider.GetUtcNow();
        var access = await accessValidator.GetDeleteExpression(userContext);

        var removed = await repository.UpdateManyByIdAsync(
            identifiers,
            UpdateLock.Delta,
            access,
            (_, old) => old.Delete(now),
            cancellationToken
        );

        if (removed.Count != identifiers.Count)
            throw new EntityNotFoundException<TIdentifier, TEntity>(identifiers);

        await publisher.PublishRemovedEvent<TIdentifier, TEntity>(userContext, identifiers, cancellationToken);
    }
}
