using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.SignalR.Contracts;

namespace Ukinee.Infrastructure.SignalR.Services;

public class ImmediateSignalRSink<TRequest, TEntity, TViewModel, THub>(
    IMapper mapper,
    IHubContext<THub> hub,
    IRouteResolver<TEntity, TRequest> routeResolver,
    ILogger<ImmediateSignalRSink<TRequest, TEntity, TViewModel, THub>> logger
) : ISignalRSink<TEntity>
where TEntity : IEntity
where THub : Hub
{
    public async Task NotifyCreation(IReadOnlyCollection<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            await NotifyGroups(entity, EntityEventType.Created, $"{typeof(TEntity).Name}OnCreated");
        }
    }

    public async Task NotifyUpdate(IReadOnlyCollection<TEntity> entities)
    {
        foreach (var entity in entities)
            await NotifyGroups(entity, EntityEventType.Updated, $"{typeof(TEntity).Name}OnUpdated");
    }

    public async Task NotifyRemoval(IReadOnlyCollection<TEntity> entities)
    {
        var method = $"{typeof(TEntity).Name}OnRemoved";

        foreach (var entity in entities)
        {
            var groups = routeResolver.GetGroupsForEntity(entity, EntityEventType.Removed).ToList();

            if (groups.Count != 0)
            {
                var identifier = entity.GetIdentifier();
                logger.LogDebug("Sending remove id {id} to {count} clients", identifier, groups.Count);

                await hub.Clients.Groups(groups).SendAsync(method, identifier);
            }
        }
    }

    private async Task NotifyGroups(TEntity entity, EntityEventType eventType, string method)
    {
        var groups = routeResolver.GetGroupsForEntity(entity, eventType).ToList();

        if (groups.Count != 0)
        {
            var viewModel = mapper.Map<TEntity, TViewModel>(entity);

            logger.LogDebug("Sending {data} to {count} clients", viewModel, groups.Count);
            await hub.Clients.Groups(groups).SendAsync(method, viewModel);
        }
    }
}
