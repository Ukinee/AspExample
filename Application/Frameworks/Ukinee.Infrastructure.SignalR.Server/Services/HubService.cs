using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public sealed class HubService<TIdentifier, TEntity, TRequest>(
    ILogger<HubService<TIdentifier, TEntity, TRequest>> logger,
    IRouteResolver<TIdentifier, TEntity, TRequest> routeResolver,
    ISignalRConnectionStateStore store,
    IServiceScopeFactory scopeFactory
) : IHubService<TRequest>
where TEntity : IEntity<TIdentifier>
where TRequest : ISignalRSubscribeRequest
{
    public async Task Subscribe(Hub hub, UserContext userContext, TRequest request)
    {
        if (!store.TryGet(hub.Context.ConnectionId, out var connectionState))
        {
            hub.Context.Abort();
            return;
        }

        if (connectionState.GroupsByMode.ContainsKey(request.Identifier))
            return;

        using var scope = scopeFactory.CreateScope();

        var signalRAccessValidator = scope.ServiceProvider.GetRequiredService<ISignalRAccessValidator<TRequest>>();

        if (!await signalRAccessValidator.ValidateAccess(request, userContext))
        {
            throw new HubException("Forbidden: You don't have access to this resource.");
        }

        logger.LogInformation("Subscribing {UserContextGuid} to {Name}", userContext.Guid, typeof(TEntity).Name);

        var groups = routeResolver.GetGroupsForRequest(request, userContext).ToList();

        connectionState.GroupsByMode.TryAdd(request.Identifier, groups.ToHashSet());

        foreach (var group in groups)
        {
            await hub.Groups.AddToGroupAsync(hub.Context.ConnectionId, group);
        }
    }

    public async Task Unsubscribe(Hub hub, UserContext userContext, Guid requestGuid)
    {
        logger.LogInformation("Unsubscribing {UserContextGuid} from {Name}", userContext.Guid, typeof(TEntity).Name);

        if (!store.TryGet(hub.Context.ConnectionId, out var connectionState))
        {
            hub.Context.Abort();
            return;
        }

        if (!connectionState.GroupsByMode.TryRemove(requestGuid, out var groups))
            return;

        foreach (var pair in connectionState.GroupsByMode.Where(x => x.Key != requestGuid))
            groups.ExceptWith(pair.Value);

        foreach (var group in groups)
        {
            await hub.Groups.RemoveFromGroupAsync(hub.Context.ConnectionId, group);
        }
    }
}
