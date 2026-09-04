namespace Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

public interface IMapService<in TResponse, TEntity>
{
    public ValueTask<TEntity> Map(TResponse externalEntity);
    public ValueTask<IReadOnlyCollection<TEntity>> Map(IReadOnlyCollection<TResponse> externalEntities);
}
