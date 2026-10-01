using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class GetOrCreateEntityUseCase<TIdentifier, TEntity, TPayload>(IEntityEnsureExistsCreator<TIdentifier, TEntity, TPayload> entityEnsureExistsCreator)
    : IGetOrCreateEntityUseCase<TIdentifier, TEntity, TPayload>
where TIdentifier : struct
where TEntity : IEntity<TIdentifier>
{
    public async Task<TEntity> Execute(
        UserContext userContext,
        TIdentifier identifier,
        TPayload payload,
        CancellationToken cancellationToken
    )
    {
        return await entityEnsureExistsCreator.GetOrCreateAsync(userContext, identifier, payload, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> Execute(
        UserContext userContext,
        IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TPayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        return await entityEnsureExistsCreator.GetOrCreateAsync(userContext, payloads, cancellationToken);
    }
}

public class TransactionGetOrCreateEntityUseCaseDecorator<TTag, TIdentifier, TEntity, TPayload>(
    IGetOrCreateEntityUseCase<TIdentifier, TEntity, TPayload> inner,
    IUnitOfWorkFactory unitOfWorkFactory
) : IGetOrCreateEntityUseCase<TIdentifier, TEntity, TPayload>
where TIdentifier : struct
where TEntity : IEntity<TIdentifier>
{
    public async Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, TPayload payload, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        var result = await inner.Execute(userContext, identifier, payload, cancellationToken);

        await uow.CommitAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyCollection<TEntity>> Execute(
        UserContext userContext,
        IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TPayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        var result = await inner.Execute(userContext, payloads, cancellationToken);

        await uow.CommitAsync(cancellationToken);

        return result;
    }
}
