using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;

public interface IIdentifierReader<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public Task<TEntity?> FindByIdAsync(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken);
    public IAsyncEnumerable<TEntity> FindManyByIdAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken);
    public Task<IReadOnlyDictionary<TIdentifier, TEntity>> GetManyByIdAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken);
}

public interface IEntityReader<in TIdentifier, out TEntity>
where TEntity : IEntity
{
    public IAsyncEnumerable<TEntity> GetAllAsync(UserContext userContext, CancellationToken cancellationToken);
}
