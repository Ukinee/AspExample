using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Utils.Extensions;
using Ukinee.Infrastructure.SignalR.Common;
using Ukinee.Infrastructure.SignalR.Server.Contracts;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public class ImmediateSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub>(
    IMapper mapper,
    IHubContext<THub> hub,
    IRouteResolver<TIdentifier, TEntity, TRequest> routeResolver,
    ILogger<ImmediateSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub>> logger
) : ISignalRSink<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
where THub : Hub
{
    public async Task NotifyCreation(IReadOnlyCollection<TEntity> entities)
    {
        await NotifyGroups(entities, EntityEventType.Created, SignalRMethodNamesUtils.Created<TEntity>());
    }

    public async Task NotifyUpdate(IReadOnlyCollection<TEntity> entities)
    {
        await NotifyGroups(entities, EntityEventType.Updated, SignalRMethodNamesUtils.Updated<TEntity>());
    }

    public async Task NotifyRemoval(IReadOnlyCollection<TIdentifier> identifiers)
    {
        var method = SignalRMethodNamesUtils.Removed<TEntity>();

        var groupsMap = identifiers.ToLookupMany(
            identifier => routeResolver.GetGroupsForIdentifier(identifier, EntityEventType.Removed),
            identifier => identifier
        );

        var sendTasks = groupsMap.Select(grouping => hub.Clients.Group(grouping.Key).SendAsync(method, grouping.ToList()));
        await Task.WhenAll(sendTasks);
    }

    private async Task NotifyGroups(IReadOnlyCollection<TEntity> entities, EntityEventType eventType, string method)
    {
        var groupsMap = entities.ToLookupMany(
            entity => routeResolver.GetGroupsForIdentifier(entity.Identifier, eventType),
            entity => mapper.Map<TViewModel>(entity)
        );

        var sendTasks = groupsMap.Select(grouping => hub.Clients.Group(grouping.Key).SendAsync(method, grouping.ToList()));
        await Task.WhenAll(sendTasks);
    }
}
