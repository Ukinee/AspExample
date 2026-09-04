using Ardalis.Specification;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;

public interface ISpecificationReader<TIdentifier, TEntity>
{
    public IAsyncEnumerable<TEntity> FindManyAsync(UserContext userContext, Specification<TEntity> specification, CancellationToken cancellationToken);
    public Task<TEntity?> FindAsync(UserContext userContext, Specification<TEntity> specification, CancellationToken cancellationToken);
}
