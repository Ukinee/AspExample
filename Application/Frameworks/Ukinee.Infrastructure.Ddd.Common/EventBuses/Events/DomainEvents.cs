using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;

public enum DomainEventAction
{
    Created,
    Updated,
    Removed,
}

public sealed class DomainEvent<TIdentifier, TEntity> : INotification
where TEntity : IEntity<TIdentifier>
{
    public required DateTimeOffset OccurredAt { get; init; }
    public required UserContext UserContext { get; init; }
    public required DomainEventAction Action { get; init; }
    public required IReadOnlyCollection<TEntity>? CreatedEntities { get; init; }
    public required IReadOnlyCollection<TEntity>? DeletedEntities { get; init; }
    public required IReadOnlyCollection<UpdateInfo<TEntity>>? UpdateInfos { get; init; }
}

public record UpdateInfo<TEntity>
{
    public UpdateInfo(TEntity updateResult, UpdateLock mode, Func<TEntity, TEntity> updateFactory)
    {
        UpdateResult = updateResult;
        Mode = mode;
        UpdateFactory = updateFactory;
    }

    public TEntity UpdateResult { get; }
    public UpdateLock Mode { get; }
    public Func<TEntity, TEntity> UpdateFactory { get; }
}

public abstract class DomainEvent
{
    public static DomainEvent<TIdentifier, TEntity> Created<TIdentifier, TEntity>(UserContext context, IReadOnlyCollection<TEntity> entities)
    where TEntity : IEntity<TIdentifier> =>
        new DomainEvent<TIdentifier, TEntity> {
            UserContext = context,
            UpdateInfos = null,
            OccurredAt = DateTimeOffset.UtcNow,
            Action = DomainEventAction.Created,
            CreatedEntities = entities,
            DeletedEntities = null,
        };

    public static DomainEvent<TIdentifier, TEntity> Updated<TIdentifier, TEntity>(UserContext context, TEntity updateResult, UpdateLock mode, Func<TEntity, TEntity> updateFactory)
    where TEntity : IEntity<TIdentifier> =>
        new DomainEvent<TIdentifier, TEntity> {
            UserContext = context,
            OccurredAt = DateTimeOffset.UtcNow,
            Action = DomainEventAction.Updated,
            UpdateInfos = [new UpdateInfo<TEntity>(updateResult, mode, updateFactory)],
            CreatedEntities = null,
            DeletedEntities = null,
        };

    public static DomainEvent<TIdentifier, TEntity> Updated<TIdentifier, TEntity>(UserContext context, IReadOnlyCollection<UpdateInfo<TEntity>> updates)
    where TEntity : IEntity<TIdentifier> =>
        new DomainEvent<TIdentifier, TEntity> {
            UserContext = context,
            OccurredAt = DateTimeOffset.UtcNow,
            Action = DomainEventAction.Updated,
            UpdateInfos = updates,
            CreatedEntities = null,
            DeletedEntities = null,
        };

    public static DomainEvent<TIdentifier, TEntity> Removed<TIdentifier, TEntity>(UserContext context, IReadOnlyCollection<TEntity> entities)
    where TEntity : IEntity<TIdentifier> =>
        new DomainEvent<TIdentifier, TEntity> {
            UserContext = context,
            OccurredAt = DateTimeOffset.UtcNow,
            Action = DomainEventAction.Removed,
            UpdateInfos = null,
            CreatedEntities = null,
            DeletedEntities = entities,
        };
}
