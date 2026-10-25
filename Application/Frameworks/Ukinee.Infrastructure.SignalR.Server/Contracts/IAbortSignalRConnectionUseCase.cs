namespace Ukinee.Infrastructure.SignalR.Server.Contracts;

public interface IAbortSignalRConnectionUseCase
{
    public Task Execute(string connectionId, CancellationToken cancellationToken);
}
