using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;

public interface IUpdateEntityUseCase<in TPayload, TEntity> // todo: to extension? 
where TEntity : class, IEntity
{
    public Task<TEntity> Execute(UserContext userContext, TEntity referenceEntity, TPayload payload, CancellationToken cancellationToken);
}

public interface IUpdateEntityUseCase<TIdentifier, TPayload, TEntity>
where TEntity : class, IEntity<TIdentifier>
{
    public Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, TPayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> batch, CancellationToken cancellationToken);
}
