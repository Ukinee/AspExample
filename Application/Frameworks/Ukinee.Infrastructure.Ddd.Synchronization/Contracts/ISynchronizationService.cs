namespace Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

// ReSharper disable once UnusedTypeParameter
public interface ISynchronizationService<T>
{
    public int Weight { get; }
    
    public Task Synchronize(CancellationToken cancellationToken);
}
