using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.SignalR.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.SignalR.Services;

public sealed class HubService<TEntity, TRequest>(
    ILogger<HubService<TEntity, TRequest>> logger,
    IRouteResolver<TEntity, TRequest> routeResolver,
    ISignalRAccessValidator<TRequest> signalRAccessValidator
) : IHubService<TRequest>
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
