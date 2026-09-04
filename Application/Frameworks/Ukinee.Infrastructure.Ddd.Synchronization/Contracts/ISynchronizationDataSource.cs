namespace Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

public interface ISynchronizationDataSource<out TEntity>
{
    public IAsyncEnumerable<TEntity> GetAll(CancellationToken cancellationToken);
}
