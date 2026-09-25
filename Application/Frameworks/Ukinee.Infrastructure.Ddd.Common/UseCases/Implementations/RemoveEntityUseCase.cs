using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class RemoveEntityUseCase<TIdentifier, TEntity>(IEntityRemover<TIdentifier, TEntity> remover) : IRemoveEntityUseCase<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public Task Execute(UserContext userContext, TEntity entity, CancellationToken cancellationToken) =>
        Execute(userContext, [entity], cancellationToken);

    public Task Execute(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken) =>
        remover.RemoveAsync(userContext, entities, cancellationToken);

    public Task Execute(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken) =>
        Execute(userContext, [identifier], cancellationToken);

    public Task Execute(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken) =>
        remover.RemoveAsync(userContext, identifiers, cancellationToken);
}
