using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCases;

public class DeltaUpdateEntityUseCase<TIdentifier, TEntity>(IDeltaEntityUpdater<TEntity> updater) : IDeltaUpdateEntityUseCase<TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public async Task<TEntity> Execute(UserContext userContext, TEntity entity, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken)
    {
        return await updater.Update(userContext, entity, updateFactory, cancellationToken);
    }
}

public class TransactionDeltaUpdateEntityUseCaseDecorator<TTag, TIdentifier, TEntity>(
    IUnitOfWorkFactory unitOfWorkFactory,
    IDeltaUpdateEntityUseCase<TEntity> inner
) : IDeltaUpdateEntityUseCase<TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public async Task<TEntity> Execute(UserContext userContext, TEntity entity, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        var result = await inner.Execute(userContext, entity, updateFactory, cancellationToken);

        await uow.CommitAsync(cancellationToken);

        return result;
    }
}
