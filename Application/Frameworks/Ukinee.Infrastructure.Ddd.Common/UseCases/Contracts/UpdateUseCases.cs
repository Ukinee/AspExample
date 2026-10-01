using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;

public interface IUpdateEntityUseCase<TIdentifier, TEntity, TPayload>
where TEntity : class, IEntity<TIdentifier>
{
    public Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, TPayload payload, CancellationToken cancellationToken);
    public Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> batch, CancellationToken cancellationToken);
}
