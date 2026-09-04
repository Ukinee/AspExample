using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Extensions;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Exceptions;
using Ukinee.Infrastructure.Ddd.Local.Extensions;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

public class TrackedReader<TIdentifier, TEntity>(
    ITrackedRepository<TIdentifier, TEntity> repository,
    IIdentifierEntityAccessValidator<TIdentifier, TEntity> identifierEntityAccessValidator
) : ITrackedReader<TIdentifier, TEntity>
where TIdentifier : notnull
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity?> FindByIdAsync(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken)
    {
        identifierEntityAccessValidator.EnsureHasAccess(userContext, identifier);

        var result = await repository.FindByIdAsync(identifier, cancellationToken);

        if (result == null)
            return null;

        return result;
    }

    public IAsyncEnumerable<TEntity> FindManyByIdAsync(UserContext userContext, IEnumerable<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        var identifiersCollection = identifiers as IReadOnlyCollection<TIdentifier> ?? identifiers.ToArray();
        
        identifierEntityAccessValidator.EnsureHasAccess(userContext, identifiersCollection);

        return repository.FindManyByIdAsync(identifiersCollection, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<TIdentifier, TEntity>> GetManyByIdAsync(UserContext userContext, IEnumerable<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        var identifierValidatorResult = identifierEntityAccessValidator.Separate(userContext, identifiers);
        var repositoryResult = await repository.Separate(identifierValidatorResult.AllowedAccess, cancellationToken);

        if (identifierValidatorResult.DeniedAccess.Count != 0 || repositoryResult.MissingIdentifiers.Count != 0)
        {
            throw new EntityNotFoundOrNotExistsException<TIdentifier, TEntity>(repositoryResult.MissingIdentifiers, identifierValidatorResult.DeniedAccess, userContext);
        }

        return repositoryResult.FoundEntities;
    }

    public IAsyncEnumerable<TEntity> GetAllAsync(UserContext userContext, CancellationToken cancellationToken)
    {
        var specification = Specification
            .For<TEntity>()
            .Where(identifierEntityAccessValidator.GetExpression(userContext))
            .Exists()
            .Specification;

        return repository.FindManyAsync(specification, cancellationToken);
    }

    public async Task<TEntity?> FindAsync(UserContext userContext, Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        specification = specification
            .Query
            .Where(identifierEntityAccessValidator.GetExpression(userContext))
            .Exists()
            .Specification;

        var result = await repository.FindAsync(specification, cancellationToken);

        return result;
    }

    public IAsyncEnumerable<TEntity> FindManyAsync(UserContext userContext, Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        specification = specification
            .Query
            .Where(identifierEntityAccessValidator.GetExpression(userContext))
            .Exists()
            .Specification;

        return repository.FindManyAsync(specification, cancellationToken);
    }
}
