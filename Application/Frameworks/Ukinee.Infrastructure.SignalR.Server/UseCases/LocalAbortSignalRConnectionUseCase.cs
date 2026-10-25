using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Services;

namespace Ukinee.Infrastructure.SignalR.Server.UseCases;

public class LocalAbortSignalRConnectionUseCase(HubSessionStore hubSessionStore) : IAbortSignalRConnectionUseCase
{
    public Task Execute(string connectionId, CancellationToken cancellationToken)
    {
        hubSessionStore.AbortConnection(connectionId);

        return Task.CompletedTask;
    }
}
