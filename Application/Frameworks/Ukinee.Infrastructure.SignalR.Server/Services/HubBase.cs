using Microsoft.AspNetCore.SignalR;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public abstract class HubBase(
    IUserContextProvider userContextProvider,
    ISignalRConnectionStateStore connectionStateStore,
    HubSessionStore hubSessionStore
) : Hub
{
    public override Task OnConnectedAsync()
    {
        var ctx = userContextProvider.GetActiveUserContext();

        connectionStateStore.GetOrCreate(Context.ConnectionId, ctx);
        hubSessionStore.RegisterConnection(Context.ConnectionId, () => Context.Abort());

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        connectionStateStore.Remove(Context.ConnectionId);
        hubSessionStore.UnregisterConnection(Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }
}
