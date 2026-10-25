using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.SignalR.Common;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Domain;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public class UserContextChangedNotificationHandler<THub>(
    ILogger<UserContextChangedNotificationHandler<THub>> logger,
    IHubContext<THub> hubContext,
    IAbortSignalRConnectionUseCase abortSignalRConnectionUseCase,
    ISignalRConnectionStateStore connectionStateStore
) : INotificationHandler<UserContextChangedNotification>
where THub : Hub
{
    private static readonly TimeSpan GracefulReconnectTimeout = TimeSpan.FromSeconds(10);

    public async Task Handle(UserContextChangedNotification notification, CancellationToken cancellationToken)
    {
        var oldContext = notification.OldContext;
        var newContext = notification.NewContext;

        if (newContext.Guid != oldContext.Guid)
        {
            logger.LogWarning("Different context guids!");
            return;
        }

        if (!connectionStateStore.TryGet(oldContext.Guid, out var connections))
            return;

        var oldConnections = connections
            .Where(c => c.UserContext.Equals(oldContext))
            .ToList();

        if (oldConnections.Count == 0)
            return;

        logger.LogInformation(
            "User context changed for {Guid}. Scheduling graceful reconnect for {Count} connection(s): {Ids}",
            oldContext.Guid,
            oldConnections.Count,
            string.Join(",", oldConnections.Select(c => c.ConnectionId))
        );

        foreach (var connection in oldConnections)
        {
            try
            {
                await hubContext
                    .Clients
                    .Client(connection.ConnectionId)
                    .SendCoreAsync(SignalRMethodNamesUtils.GracefulReconnect(), [], cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send GracefulReconnect to {ConnectionId}", connection.ConnectionId);
            }
        }

        await Task.Delay(GracefulReconnectTimeout, cancellationToken);

        if (!connectionStateStore.TryGet(newContext.Guid, out var current))
        {
            return;
        }

        var stillStaleIds = current
            .Where(c => c.UserContext.Equals(oldContext))
            .Select(c => c.ConnectionId)
            .ToHashSet();

        if (stillStaleIds.Count == 0)
        {
            logger.LogInformation(
                "All {Count} connection(s) for {Guid} gracefully reconnected",
                oldConnections.Count,
                oldContext.Guid
            );

            return;
        }

        logger.LogInformation(
            "Aborting {Count} stale connection(s) for {Guid}: {Ids}",
            stillStaleIds.Count,
            oldContext.Guid,
            string.Join(",", stillStaleIds)
        );

        foreach (var connectionId in stillStaleIds)
        {
            try
            {
                await abortSignalRConnectionUseCase.Execute(connectionId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to abort connection {ConnectionId}", connectionId);
            }
        }
    }
}
