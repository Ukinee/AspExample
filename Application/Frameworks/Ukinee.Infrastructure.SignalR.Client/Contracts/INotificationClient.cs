using Microsoft.AspNetCore.SignalR.Client;

namespace Ukinee.Infrastructure.SignalR.Client.Contracts;

public interface INotificationClient<THubTag>
{
    public HubConnectionState State { get; }

    public void RegisterHandlerAsync(); // to connect?
    public void UnregisterHandlerAsync();

    public Task SubscribeAsync<TEntity, TPayload>(TPayload payload);
    public Task UnsubscribeAsync<TEntity, TPayload>(TPayload payload);

    public Task ConnectAsync(CancellationToken cancellationToken);
    public Task DisconnectAsync(CancellationToken cancellationToken);
}