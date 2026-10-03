using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCases.Contracts;

public interface IDeltaUpdateEntityUseCase<in TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
{
    public Task<TEntity> Execute(UserContext userContext, TIdentifier identifier,  Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken);
}
