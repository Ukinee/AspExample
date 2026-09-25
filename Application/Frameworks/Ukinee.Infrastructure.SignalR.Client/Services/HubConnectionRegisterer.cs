using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Extensions;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Domain;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Infrastructure.SignalR.Client.Contracts;
using Ukinee.Infrastructure.SignalR.Common;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.SignalR.Client.Services;

public class HubConnectionRegisterer<THubTag, TIdentifier, TEntity, TResponse>(
    IMapService<TResponse, TEntity> mapService,
    IUserContextProvider userContextProvider,
    IMediator mediator
) : IHubConnectionRegisterer<THubTag>
where TEntity : IEntity<TIdentifier>
{
    public void Register(HubConnection connection)
    {
        var removed = SignalRMethodNamesUtils.Removed<TEntity>();
        var updated = SignalRMethodNamesUtils.Updated<TEntity>();
        var created = SignalRMethodNamesUtils.Created<TEntity>();
        var upsert = SignalRMethodNamesUtils.Upsert<TEntity>();

        connection.On<IReadOnlyCollection<TIdentifier>>(removed, HandleRemoved);
        connection.On<IReadOnlyCollection<TResponse>>(updated, HandleUpdated);
        connection.On<IReadOnlyCollection<TResponse>>(created, HandleCreated);
        connection.On<IReadOnlyCollection<TResponse>>(upsert, HandleUpsert);
    }

    public void Unregister(HubConnection connection)
    {
        var removed = SignalRMethodNamesUtils.Removed<TEntity>();
        var updated = SignalRMethodNamesUtils.Updated<TEntity>();
        var created = SignalRMethodNamesUtils.Created<TEntity>();
        var upsert = SignalRMethodNamesUtils.Upsert<TEntity>();

        connection.Remove(removed);
        connection.Remove(updated);
        connection.Remove(created);
        connection.Remove(upsert);
    }

    private async Task HandleUpsert(IReadOnlyCollection<TResponse> responses)
    {
        var entities = await mapService.Map(responses);
        var userContext = userContextProvider.GetActiveUserContext();

        var command = new UpsertCachedEntitiesCommand<TIdentifier, TEntity>(userContext, entities);

        await mediator.Send(command);
        await mediator.PublishClientUpsertEvent<TIdentifier, TEntity>(userContext, entities, CancellationToken.None);
    }

    private async Task HandleCreated(IReadOnlyCollection<TResponse> responses)
    {
        var entities = await mapService.Map(responses);
        var userContext = userContextProvider.GetActiveUserContext();

        var command = new UpsertCachedEntitiesCommand<TIdentifier, TEntity>(userContext, entities);

        await mediator.Send(command);
        await mediator.PublishCreatedEvent<TIdentifier, TEntity>(userContext, entities, CancellationToken.None);
    }

    private async Task HandleUpdated(IReadOnlyCollection<TResponse> responses)
    {
        var entities = await mapService.Map(responses);

        var updates = entities
            .Select(entity => new UpdateResult<TEntity>(entity, default))
            .ToList();

        var userContext = userContextProvider.GetActiveUserContext();

        var command = new UpsertCachedEntitiesCommand<TIdentifier, TEntity>(userContext, entities);

        await mediator.Send(command);
        await mediator.PublishUpdatedEvent<TIdentifier, TEntity>(userContext, updates, CancellationToken.None);
    }

    private async Task HandleRemoved(IReadOnlyCollection<TIdentifier> identifiers)
    {
        var userContext = userContextProvider.GetActiveUserContext();

        var command = new RemoveCachedEntitiesCommand<TIdentifier, TEntity>(userContext, identifiers);

        await mediator.Send(command);
        await mediator.PublishClientRemovedEvent<TIdentifier, TEntity>(userContext, identifiers, CancellationToken.None);
    }
}
