using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.SignalR.Client.Contracts;
using Ukinee.Infrastructure.SignalR.Common;

namespace Ukinee.Infrastructure.SignalR.Client.Services;

public class NotificationClient<THubTag>(
    ILogger<NotificationClient<THubTag>> logger,
    IEnumerable<IHubConnectionRegisterer<THubTag>> registerers,
    IHubConnectionFactory<THubTag> factory
) : INotificationClient<THubTag>, IAsyncDisposable
{
    private readonly HubConnection _connection = factory.Create();

    public HubConnectionState State => _connection.State;

    public async Task RegisterHandlerAsync()
    {
        foreach (var registerer in registerers)
            registerer.Register(_connection);
    }

    public async Task UnregisterHandlerAsync()
    {
        foreach (var registerer in registerers)
             registerer.Unregister(_connection);
    }

    public async Task SubscribeAsync<TEntity, TPayload>(TPayload payload)
    {
        var method = SignalRMethodNamesUtils.Subscribe<TEntity>();

        await _connection.InvokeAsync(method, payload);
    }

    public async Task UnsubscribeAsync<TEntity, TPayload>(TPayload payload)
    {
        var method = SignalRMethodNamesUtils.Unsubscribe<TEntity>();

        await _connection.InvokeAsync(method, payload);
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync(cancellationToken);
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        if (State != HubConnectionState.Disconnected)
        {
            await _connection.StopAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
