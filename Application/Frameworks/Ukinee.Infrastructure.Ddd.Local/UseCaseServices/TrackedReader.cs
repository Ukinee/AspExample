using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

public class TrackedReader<TIdentifier, TEntity>(
    IIdentifierReader<TIdentifier, TEntity> identifierReader,
    IAllEntityReader<TIdentifier, TEntity> allEntityReader,
    ISpecificationReader<TIdentifier, TEntity> specificationReader
) : ITrackedReader<TIdentifier, TEntity>
where TIdentifier : struct
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity?> FindByIdAsync(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken)
    {
        return await identifierReader.FindByIdAsync(userContext, identifier, cancellationToken);
    }

    public IAsyncEnumerable<TEntity> FindManyByIdAsync(
        UserContext userContext,
        IReadOnlyCollection<TIdentifier> identifiers,
        CancellationToken cancellationToken
    )
    {
        return identifierReader.FindManyByIdAsync(userContext, identifiers, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<TIdentifier, TEntity>> GetManyByIdAsync(
        UserContext userContext,
        IReadOnlyCollection<TIdentifier> identifiers,
        CancellationToken cancellationToken
    )
    {
        return await identifierReader.GetManyByIdAsync(userContext, identifiers, cancellationToken);
    }

    public IAsyncEnumerable<TEntity> GetAllAsync(UserContext userContext, CancellationToken cancellationToken)
    {
        return allEntityReader.GetAllAsync(userContext, cancellationToken);
    }

    public async Task<TEntity?> FindAsync(UserContext userContext, Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        return await specificationReader.FindAsync(userContext, specification, cancellationToken);
    }

    public IAsyncEnumerable<TEntity> FindManyAsync(
        UserContext userContext,
        Specification<TEntity> specification,
        CancellationToken cancellationToken
    )
    {
        return specificationReader.FindManyAsync(userContext, specification, cancellationToken);
    }
}
