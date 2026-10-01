using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class RemoveEntityUseCase<TIdentifier, TEntity>(IEntityRemover<TIdentifier, TEntity> remover) : IRemoveEntityUseCase<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public Task Execute(UserContext userContext, TEntity entity, CancellationToken cancellationToken) =>
        Execute(userContext, [entity], cancellationToken);

    public async Task Execute(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        await remover.RemoveAsync(userContext, entities, cancellationToken);
    }

    public Task Execute(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken) =>
        Execute(userContext, [identifier], cancellationToken);

    public async Task Execute(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        await remover.RemoveAsync(userContext, identifiers, cancellationToken);
    }
}

public class TransactionRemoveEntityUseCaseDecorator<TTag, TIdentifier, TEntity>(
    IRemoveEntityUseCase<TIdentifier, TEntity> inner,
    IUnitOfWorkFactory unitOfWorkFactory
) : IRemoveEntityUseCase<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task Execute(UserContext userContext, TEntity entity, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        await inner.Execute(userContext, entity, cancellationToken);

        await uow.CommitAsync(cancellationToken);
    }

    public async Task Execute(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        await inner.Execute(userContext, entities, cancellationToken);

        await uow.CommitAsync(cancellationToken);
    }

    public async Task Execute(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        await inner.Execute(userContext, identifier, cancellationToken);

        await uow.CommitAsync(cancellationToken);
    }

    public async Task Execute(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        await inner.Execute(userContext, identifiers, cancellationToken);

        await uow.CommitAsync(cancellationToken);
    }
}
