using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.Synchronization.Implementations;

public class SynchronizationService<TIdentifier, TEntity, TWeight>(
    ILogger<SynchronizationService<TIdentifier, TEntity, TWeight>> logger,
    ISynchronizationDataSource<TEntity> synchronizationDataSource,
    IEditableTrackedRepository<TIdentifier, TEntity> trackedRepository
) : ISynchronizationService<TEntity>
where TWeight : ISynchronizationOrderByPriority, allows ref struct
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public int Weight => TWeight.Value;

    public async Task Synchronize(CancellationToken cancellationToken)
    {
        logger.LogInformation("Synchronizing data for {Type}...", typeof(TEntity));

        if (synchronizationDataSource == trackedRepository)
        {
            throw new InvalidOperationException($"The data source IS repository! No need to call {nameof(SynchronizationService<,,>)}.");
        }

        var result = new List<TEntity>();

        await foreach (var externalEntity in synchronizationDataSource.GetAll(cancellationToken))
        {
            result.Add(externalEntity);
        }

        await trackedRepository.AddRange(result);

        logger.LogInformation("Data for {Type} synchronized.", typeof(TEntity));
    }
}
