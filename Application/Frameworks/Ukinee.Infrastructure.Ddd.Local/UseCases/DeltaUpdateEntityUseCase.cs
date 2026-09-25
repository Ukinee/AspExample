using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCases;

public class DeltaUpdateEntityUseCase<TIdentifier, TEntity>(IDeltaEntityUpdater<TEntity> updater) : IDeltaUpdateEntityUseCase<TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task<TEntity> Execute(UserContext userContext, TEntity entity, UpdateLock mode, Func<TEntity, TEntity> updateFactory) =>
        updater.Update(userContext, entity, mode, updateFactory, CancellationToken.None);
}
