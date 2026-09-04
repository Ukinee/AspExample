using MediatR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.EventBuses.Extensions;

public static class EventBusExtensions
{
    extension(IPublisher bus)
    {
        public Task PublishCreatedEvent<TIdentifier, TEntity>(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(DomainEvent.Created<TIdentifier, TEntity>(userContext, entities));
        }

        public Task PublishRemovedEvent<TIdentifier, TEntity>(UserContext userContext, IReadOnlyCollection<TEntity> entities)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(DomainEvent.Removed<TIdentifier, TEntity>(userContext, entities));
        }

        public Task PublishUpdatedEvent<TIdentifier, TEntity>(UserContext userContext, TEntity entity, UpdateLock mode, Func<TEntity, TEntity> updateFactory)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(DomainEvent.Updated<TIdentifier, TEntity>(userContext, entity, mode, updateFactory));
        }
        
        public Task PublishUpdatedEvent<TIdentifier, TEntity>(UserContext userContext, IReadOnlyCollection<UpdateInfo<TEntity>> updates)
        where TEntity : IEntity<TIdentifier>
        {
            return bus.Publish(DomainEvent.Updated<TIdentifier, TEntity>(userContext, updates));
        }
    }
}
