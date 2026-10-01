using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class UpdateEntityUseCase<TIdentifier, TEntity, TPayload>(IPayloadEntityUpdater<TIdentifier, TEntity, TPayload> updater)
    : IUpdateEntityUseCase<TIdentifier, TEntity, TPayload>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public async Task<TEntity> Execute(UserContext userContext, TIdentifier identfier, TPayload payload, CancellationToken cancellationToken)
    {
        return await updater.UpdateAsync(userContext, identfier, payload, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> Execute(
        UserContext userContext,
        IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        return await updater.UpdateAsync(userContext, payloads, cancellationToken);
    }
}

public class TransactionUpdateEntityUseCaseDecorator<TTag, TIdentifier, TEntity, TPayload>(
    IUpdateEntityUseCase<TIdentifier, TEntity, TPayload> inner,
    IUnitOfWorkFactory unitOfWorkFactory
) : IUpdateEntityUseCase<TIdentifier, TEntity, TPayload>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public async Task<TEntity> Execute(UserContext userContext, TIdentifier identfier, TPayload payload, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        var result = await inner.Execute(userContext, identfier, payload, cancellationToken);

        await uow.CommitAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyCollection<TEntity>> Execute(
        UserContext userContext,
        IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TPayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        var result = await inner.Execute(userContext, payloads, cancellationToken);

        await uow.CommitAsync(cancellationToken);

        return result;
    }
}
