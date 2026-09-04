using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

public class HonestTrackedRemover<TIdentifier, TEntity>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IIdentifierEntityAccessValidator<TIdentifier, TEntity> identifierEntityAccessValidator,
    IPublisher publisher
) : IEntityRemover<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        identifierEntityAccessValidator.EnsureHasAccess(userContext, entities);

        var removedEntities = await repository.RemoveRange(entities.Select(e => e.Identifier));

        await publisher.PublishRemovedEvent<TIdentifier, TEntity>(userContext, removedEntities);
    }

    public async Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        identifierEntityAccessValidator.EnsureHasAccess(userContext, identifiers);

        var removedEntities = await repository.RemoveRange(identifiers);

        await publisher.PublishRemovedEvent<TIdentifier, TEntity>(userContext, removedEntities);
    }
}

//todo: batch update
public class DeletableTrackedRemover<TIdentifier, TEntity>(
    IEditableTrackedRepository<TIdentifier, TEntity> repository,
    IIdentifierEntityAccessValidator<TIdentifier, TEntity> accessValidator,
    IPublisher publisher,
    TimeProvider timeProvider
) : IEntityRemover<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>, ISpecificationForSoftDelete<TEntity>
{
    public async Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var result = new List<TEntity>(entities.Count);

        var identifiers = entities.Select(e => e.Identifier).ToList();

        accessValidator.EnsureHasAccess(userContext, identifiers);

        foreach (var identifier in identifiers)
        {
            var updated = await repository.UpdateByIdAsync(identifier, UpdateLock.Delta, old => old.Delete(now), cancellationToken);
            result.Add(updated);
        }

        await publisher.PublishRemovedEvent<TIdentifier, TEntity>(userContext, result);
    }

    public async Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var result = new List<TEntity>(identifiers.Count);

        accessValidator.EnsureHasAccess(userContext, identifiers);

        foreach (var identifier in identifiers)
        {
            var updated = await repository.UpdateByIdAsync(identifier, UpdateLock.Delta, old => old.Delete(now), cancellationToken);
            result.Add(updated);
        }

        await publisher.PublishRemovedEvent<TIdentifier, TEntity>(userContext, result);
    }
}
