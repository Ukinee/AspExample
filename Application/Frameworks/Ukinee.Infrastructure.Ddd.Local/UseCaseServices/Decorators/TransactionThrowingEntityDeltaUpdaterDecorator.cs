using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Decorators;

public class TransactionThrowingEntityDeltaUpdaterDecorator<TIdentifier, TEntity>(
    IUnitOfWorkProvider uowProvider,
    IEntityDeltaUpdater<TIdentifier, TEntity> inner
) : IEntityDeltaUpdater<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
{
    public Task<TEntity> Update(UserContext userContext, TIdentifier identifier, Func<TEntity, TEntity> updateFactory, CancellationToken cancellationToken)
    {
        if (uowProvider.Current != null)
            throw new NotSupportedException($"Transactions not supported for {typeof(TEntity).Name}");

        return inner.Update(userContext, identifier, updateFactory, cancellationToken);
    }
}
