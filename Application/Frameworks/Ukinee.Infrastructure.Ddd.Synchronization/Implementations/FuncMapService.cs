using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.Synchronization.Implementations;

public class FuncMapService<TSource, TEntity>(Func<TSource, TEntity> factory) : IMapService<TSource, TEntity>
{
    public ValueTask<TEntity> Map(TSource externalEntity) =>
        ValueTask.FromResult(factory.Invoke(externalEntity));

    public async ValueTask<IReadOnlyCollection<TEntity>> Map(IReadOnlyCollection<TSource> externalEntities) =>
        externalEntities.Select(factory.Invoke).ToList();
}