using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;

public record UpsertClientDomainEvent<TIdentifier, TEntity> : DomainEvent<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required IReadOnlyCollection<TEntity> UpsertEntities { get; init; }
}

public record DeletedClientDomainEvent<TIdentifier, TEntity> : DomainEvent<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required IReadOnlyCollection<TIdentifier> DeletedIdentifiers { get; init; }
}

public class ClientDomainEvents
{
    public static DeletedClientDomainEvent<TIdentifier, TEntity> Removed<TIdentifier, TEntity>(UserContext context, IReadOnlyCollection<TIdentifier> identifiers)
    where TEntity : IEntity<TIdentifier> =>
        new DeletedClientDomainEvent<TIdentifier, TEntity> {
            UserContext = context,
            OccurredAt = DateTimeOffset.UtcNow,
            DeletedIdentifiers = identifiers,
        };

    public static UpsertClientDomainEvent<TIdentifier, TEntity> Upsert<TIdentifier, TEntity>(UserContext context, IReadOnlyCollection<TEntity> entities)
    where TEntity : IEntity<TIdentifier> =>
        new UpsertClientDomainEvent<TIdentifier, TEntity> {
            UserContext = context,
            OccurredAt = DateTimeOffset.UtcNow,
            UpsertEntities = entities,
        };
}
