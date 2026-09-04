using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;

public interface IEntityRemover<in TIdentifier, in TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken);
    public Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken);
}