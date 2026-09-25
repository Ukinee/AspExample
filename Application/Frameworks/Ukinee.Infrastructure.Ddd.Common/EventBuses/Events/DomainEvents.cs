using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;

public sealed record UpdateResult<TEntity>(TEntity Current, TEntity? Previous);

public record DomainEvent<TIdentifier, TEntity> : INotification
where TEntity : IEntity<TIdentifier>
{
    public required DateTimeOffset OccurredAt { get; init; }
    public required UserContext UserContext { get; init; }
}

public record CreatedDomainEvent<TIdentifier, TEntity> : DomainEvent<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required IReadOnlyCollection<TEntity> Entities { get; init; }
}

public record UpdatedDomainEvent<TIdentifier, TEntity> : DomainEvent<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required IReadOnlyCollection<UpdateResult<TEntity>> Updates { get; init; }
}

public record DeletedDomainEvent<TIdentifier, TEntity> : DomainEvent<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required IReadOnlyCollection<TIdentifier> Identifiers { get; init; }
}

public static class DomainEvent
{
    public static DomainEvent<TIdentifier, TEntity> Created<TIdentifier, TEntity>(UserContext context, IReadOnlyCollection<TEntity> entities)
    where TEntity : IEntity<TIdentifier> =>
        new CreatedDomainEvent<TIdentifier, TEntity> {
            UserContext = context,
            OccurredAt = DateTimeOffset.UtcNow,
            Entities = entities,
        };

    public static DomainEvent<TIdentifier, TEntity> Updated<TIdentifier, TEntity>(UserContext context, TEntity updateResult, TEntity previous)
    where TEntity : IEntity<TIdentifier>
    {
        var info = new UpdateResult<TEntity>(updateResult, previous);

        return Updated<TIdentifier, TEntity>(context, [info]);
    }

    public static DomainEvent<TIdentifier, TEntity> Updated<TIdentifier, TEntity>(UserContext context, IReadOnlyCollection<UpdateResult<TEntity>> updates)
    where TEntity : IEntity<TIdentifier> =>
        new UpdatedDomainEvent<TIdentifier, TEntity> {
            UserContext = context,
            OccurredAt = DateTimeOffset.UtcNow,
            Updates = updates,
        };

    public static DomainEvent<TIdentifier, TEntity> Removed<TIdentifier, TEntity>(UserContext context, IReadOnlyCollection<TIdentifier> identifiers)
    where TEntity : IEntity<TIdentifier> =>
        new DeletedDomainEvent<TIdentifier, TEntity> {
            UserContext = context,
            OccurredAt = DateTimeOffset.UtcNow,
            Identifiers = identifiers,
        };
}
