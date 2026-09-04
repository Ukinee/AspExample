using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;

public interface IUpdateEntityUseCase<TEntity>
where TEntity : class, IEntity
{
    public Task<TEntity> Execute(UserContext userContext, TEntity entity, UpdateLock mode, Func<TEntity, TEntity> updateFactory);
}

public interface IUpdateEntityUseCase<in TPayload, TEntity> // todo: to extension? 
where TEntity : class, IEntity
{
    public Task<TEntity> Execute(UserContext userContext, TEntity referenceEntity, TPayload payload);
}

public interface IUpdateEntityUseCase<TIdentifier, TPayload, TEntity>
where TEntity : class, IEntity<TIdentifier>
{
    public Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, TPayload payload);
    public Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> batch);
}
