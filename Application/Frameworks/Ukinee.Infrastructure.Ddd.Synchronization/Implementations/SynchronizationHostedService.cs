using Ukinee.Infrastructure.Common.Contracts;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.Synchronization.Implementations;

public class SynchronizationHostedService<TEntity>(ISynchronizationService<TEntity> syncService) : IOrderedHostedService
{
    public int Weight => syncService.Weight;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await syncService.Synchronize(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
