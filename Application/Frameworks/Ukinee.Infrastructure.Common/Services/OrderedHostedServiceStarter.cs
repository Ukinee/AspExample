using Microsoft.Extensions.Hosting;
using Ukinee.Infrastructure.Common.Contracts;

namespace Ukinee.Infrastructure.Common.Services;

public class OrderedHostedServiceStarter(IEnumerable<IOrderedHostedService> orderedHostedServices) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var orderedHostedService in orderedHostedServices.OrderBy(d => d.OrderByPriority))
        {
            await orderedHostedService.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var orderedHostedService in orderedHostedServices.OrderByDescending(d => d.OrderByPriority))
        {
            await orderedHostedService.StopAsync(cancellationToken);
        }
    }
}
