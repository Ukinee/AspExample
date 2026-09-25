using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;

public interface IDeltaEntityUpdater<TEntity>
where TEntity : class, IEntity
{
    public Task<TEntity> Update(UserContext userContext, TEntity entity, UpdateLock mode, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken);
}
