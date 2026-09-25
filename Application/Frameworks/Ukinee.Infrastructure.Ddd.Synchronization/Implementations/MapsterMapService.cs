using MapsterMapper;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.Synchronization.Implementations;

public class MapsterMapService<TSource, TEntity>(IMapper mapper) : IMapService<TSource, TEntity>
where TEntity : notnull
where TSource : notnull
{
    public async ValueTask<TEntity> Map(TSource externalEntity)
    {
        return mapper.Map<TEntity>(externalEntity);
    }

    public async ValueTask<IReadOnlyCollection<TEntity>> Map(IReadOnlyCollection<TSource> externalEntities)
    {
        return mapper.Map<IReadOnlyCollection<TEntity>>(externalEntities);
    }
}
