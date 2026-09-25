using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCases.Contracts;

public interface IDeltaUpdateEntityUseCase<TEntity>
where TEntity : class, IEntity
{
    public Task<TEntity> Execute(UserContext userContext, TEntity entity, UpdateLock mode, Func<TEntity, TEntity> updateFactory);
}
