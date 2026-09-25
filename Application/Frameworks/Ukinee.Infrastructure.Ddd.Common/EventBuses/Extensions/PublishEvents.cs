using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.EventBuses.Extensions;

public static class EventBusExtensions
{
    extension(IPublisher bus)
    {
        public Task PublishCreatedEvent<TIdentifier, TEntity>(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellation)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(DomainEvent.Created<TIdentifier, TEntity>(userContext, entities), cancellation);
        }

        public Task PublishRemovedEvent<TIdentifier, TEntity>(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellation)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(DomainEvent.Removed<TIdentifier, TEntity>(userContext, identifiers), cancellation);
        }

        public Task PublishUpdatedEvent<TIdentifier, TEntity>(UserContext userContext, TEntity current, TEntity? previous, CancellationToken cancellation)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(DomainEvent.Updated<TIdentifier, TEntity>(userContext, current, previous), cancellation);
        }

        public Task PublishUpdatedEvent<TIdentifier, TEntity>(UserContext userContext, IReadOnlyCollection<UpdateResult<TEntity>> updates, CancellationToken cancellation)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(DomainEvent.Updated<TIdentifier, TEntity>(userContext, updates), cancellation);
        }

        public Task PublishClientRemovedEvent<TIdentifier, TEntity>(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellation)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(ClientDomainEvents.Removed<TIdentifier, TEntity>(userContext, identifiers), cancellation);
        }

        public Task PublishClientUpsertEvent<TIdentifier, TEntity>(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellation)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(ClientDomainEvents.Upsert<TIdentifier, TEntity>(userContext, entities), cancellation);
        }
    }
}
