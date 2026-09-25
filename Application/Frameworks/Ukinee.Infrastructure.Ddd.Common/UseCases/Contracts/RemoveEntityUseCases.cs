using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;

public interface IRemoveEntityUseCase<in TEntity>
where TEntity : IEntity
{
    public Task Execute(UserContext userContext, TEntity entity, CancellationToken cancellationToken);
    public Task Execute(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken);
}

public interface IRemoveEntityUseCase<in TIdentifier, in TEntity> : IRemoveEntityUseCase<TEntity>
where TEntity : IEntity<TIdentifier>
{
    public Task Execute(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken);
    public Task Execute(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken);
}
