using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Decorators;

public class TransactionThrowingDeltaEntityUpdater<TEntity>(
    IUnitOfWorkProvider uowProvider,
    IDeltaEntityUpdater<TEntity> inner
) : IDeltaEntityUpdater<TEntity>
where TEntity : class, IEntity
{
    public Task<TEntity> Update(UserContext userContext, TEntity entity, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken)
    {
        if (uowProvider.Current != null)
            throw new InvalidOperationException($"Transactions not supported for {typeof(TEntity).Name}");

        return inner.Update(userContext, entity, updateFactory, cancellationToken);
    }
}
