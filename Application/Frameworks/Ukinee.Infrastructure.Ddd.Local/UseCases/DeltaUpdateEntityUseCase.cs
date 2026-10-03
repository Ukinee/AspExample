using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCases;

public class DeltaUpdateEntityUseCase<TIdentifier, TEntity>(IDeltaEntityUpdater<TIdentifier, TEntity> updater) : IDeltaUpdateEntityUseCase<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public async Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken)
    {
        return await updater.Update(userContext, identifier, updateFactory, cancellationToken);
    }
}

public class TransactionDeltaUpdateEntityUseCaseDecorator<TTag, TIdentifier, TEntity>(
    IUnitOfWorkFactory unitOfWorkFactory,
    IDeltaUpdateEntityUseCase<TIdentifier, TEntity> inner
) : IDeltaUpdateEntityUseCase<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public async Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken)
    {
        await using var uow = unitOfWorkFactory.Create<TTag>();

        var result = await inner.Execute(userContext, identifier, updateFactory, cancellationToken);

        await uow.CommitAsync(cancellationToken);

        return result;
    }
}
