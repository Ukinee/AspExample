using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public sealed class HubService<TIdentifier, TEntity, TRequest>(
    ILogger<HubService<TIdentifier, TEntity, TRequest>> logger,
    IRouteResolver<TIdentifier, TEntity, TRequest> routeResolver,
    ISignalRAccessValidator<TRequest> signalRAccessValidator
) : IHubService<TRequest>
where TEntity : IEntity<TIdentifier>
{
    public async Task Subscribe(Hub hub, UserContext userContext, TRequest request)
    {
        if (!await signalRAccessValidator.ValidateAccess(request, userContext))
        {
            throw new HubException("Forbidden: You don't have access to this resource.");
        }

        logger.LogInformation("Subscribing {UserContextGuid} to {Name}", userContext.Guid, typeof(TEntity).Name);

        var groups = routeResolver.GetGroupsForRequest(request, userContext);

        foreach (var group in groups)
        {
            await hub.Groups.AddToGroupAsync(hub.Context.ConnectionId, group);
        }
    }

    public async Task Unsubscribe(Hub hub, UserContext userContext, TRequest request)
    {
        logger.LogInformation("Unsubscribing {UserContextGuid} from {Name}", userContext.Guid, typeof(TEntity).Name);

        var groups = routeResolver.GetGroupsForRequest(request, userContext);

        foreach (var group in groups)
        {
            await hub.Groups.RemoveFromGroupAsync(hub.Context.ConnectionId, group);
        }
    }
}
