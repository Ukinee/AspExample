using Microsoft.AspNetCore.SignalR.Client;

namespace Ukinee.Infrastructure.SignalR.Client.Contracts;

public interface IHubConnectionRegisterer<THubTag>
{
    public void Register(HubConnection connection);
    public void Unregister(HubConnection connection);
}
