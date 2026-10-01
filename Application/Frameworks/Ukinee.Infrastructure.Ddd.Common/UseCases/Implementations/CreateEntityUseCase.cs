using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases;

public class CreateEntityUseCase<TEntity, TPayload>(IEntityCreator<TEntity, TPayload> entityCreator)
    : ICreateEntityUseCase<TEntity, TPayload>
where TEntity : class, IEntity
{
    public virtual async Task<TEntity> Execute(UserContext userContext, TPayload payload, CancellationToken cancellationToken)
    {
        return await entityCreator.CreateAsync(userContext, payload, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<TPayload> payloads, CancellationToken cancellationToken)
    {
        return await entityCreator.CreateAsync(userContext, payloads, cancellationToken);
    }
}

public class TransactionCreateEntityUseCaseDecorator<TTag, TEntity, TPayload>(
    ICreateEntityUseCase<TEntity, TPayload> inner,
    IUnitOfWorkFactory unitOfWorkFactory
) : ICreateEntityUseCase<TEntity, TPayload>
where TEntity : IEntity
{
    public async Task<TEntity> Execute(UserContext userContext, TPayload payload, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        var result = await inner.Execute(userContext, payload, cancellationToken);

        await uow.CommitAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyCollection<TEntity>> Execute(UserContext userContext, IReadOnlyCollection<TPayload> payloads, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        var result = await inner.Execute(userContext, payloads, cancellationToken);

        await uow.CommitAsync(cancellationToken);

        return result;
    }
}
