using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.Synchronization.Implementations;

public class PassingMapService<TEntity> : IMapService<TEntity, TEntity>
{
    public ValueTask<TEntity> Map(TEntity externalEntity) =>
        ValueTask.FromResult(externalEntity);

    public async ValueTask<IReadOnlyCollection<TEntity>> Map(IReadOnlyCollection<TEntity> externalEntities) =>
        await Task.FromResult(externalEntities);
}