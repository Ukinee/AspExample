using Microsoft.AspNetCore.SignalR.Client;

namespace Ukinee.Infrastructure.SignalR.Client.Contracts;

public interface IHubConnectionFactory<THubTag>
{
    public HubConnection Create();
}
