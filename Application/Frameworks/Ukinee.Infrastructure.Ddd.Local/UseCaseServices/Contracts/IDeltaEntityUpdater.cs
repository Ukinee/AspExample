using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;

public interface IDeltaEntityUpdater<in TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
{
    public Task<TEntity> Update(UserContext userContext, TIdentifier identifier,  Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken);
}
