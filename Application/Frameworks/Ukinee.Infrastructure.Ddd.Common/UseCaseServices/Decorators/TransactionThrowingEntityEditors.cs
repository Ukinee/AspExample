using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Decorators;

public class TransactionThrowingEntityCreator<TCreatePayload, TEntity>(
    IUnitOfWorkProvider uowProvider,
    IEntityCreator<TCreatePayload, TEntity> inner
) : IEntityCreator<TCreatePayload, TEntity>
where TEntity : IEntity
{
    public Task<TEntity> CreateAsync(UserContext userContext, TCreatePayload payload, CancellationToken cancellationToken)
    {
        if (uowProvider.Current != null)
            throw new InvalidOperationException($"Transactions not supported for {typeof(TEntity).Name}");

        return inner.CreateAsync(userContext, payload, cancellationToken);
    }

    public Task<IReadOnlyCollection<TEntity>> CreateAsync(
        UserContext userContext,
        IReadOnlyCollection<TCreatePayload> payloads,
        CancellationToken cancellationToken
    )
    {
        if (uowProvider.Current != null)
            throw new InvalidOperationException($"Transactions not supported for {typeof(TEntity).Name}");

        return inner.CreateAsync(userContext, payloads, cancellationToken);
    }
}

public class TransactionThrowingEntityUpdater<TIdentifier, TEntity, TUpdatePayload>(
    IUnitOfWorkProvider uowProvider,
    IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity> inner
) : IPayloadEntityUpdater<TIdentifier, TUpdatePayload, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task<TEntity> UpdateAsync(UserContext userContext, TIdentifier identifier, TUpdatePayload payload, CancellationToken cancellationToken)
    {
        if (uowProvider.Current != null)
            throw new InvalidOperationException($"Transactions not supported for {typeof(TEntity).Name}");

        return inner.UpdateAsync(userContext, identifier, payload, cancellationToken);
    }

    public Task<IReadOnlyCollection<TEntity>> UpdateAsync(
        UserContext userContext,
        IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TUpdatePayload>> payloads,
        CancellationToken cancellationToken
    )
    {
        if (uowProvider.Current != null)
            throw new InvalidOperationException($"Transactions not supported for {typeof(TEntity).Name}");

        return inner.UpdateAsync(userContext, payloads, cancellationToken);
    }
}

public class TransactionThrowingEntityRemover<TIdentifier, TEntity>(
    IUnitOfWorkProvider uowProvider,
    IEntityRemover<TIdentifier, TEntity> inner
) : IEntityRemover<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : notnull
{
    public Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TEntity> entities, CancellationToken cancellationToken)
    {
        if (uowProvider.Current != null)
            throw new InvalidOperationException($"Transactions not supported for {typeof(TEntity).Name}");

        return inner.RemoveAsync(userContext, entities, cancellationToken);
    }

    public Task RemoveAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        if (uowProvider.Current != null)
            throw new InvalidOperationException($"Transactions not supported for {typeof(TEntity).Name}");

        return inner.RemoveAsync(userContext, identifiers, cancellationToken);
    }
}
