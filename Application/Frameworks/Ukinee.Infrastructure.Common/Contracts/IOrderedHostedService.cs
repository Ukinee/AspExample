namespace Ukinee.Infrastructure.Common.Contracts;

public interface IOrderedHostedService
{
    public int Weight { get; }
    
    public Task StartAsync(CancellationToken cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken);
}
