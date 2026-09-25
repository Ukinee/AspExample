using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ardalis.Specification;
using Ukinee.DbAccess.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Utils;
using Ukinee.Infrastructure.Ddd.Common.Utils.Extensions;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.UseCaseServices;

public class TrackedReader<TIdentifier, TEntity>(
    ITrackedRepository<TIdentifier, TEntity> repository,
    IEntityReadAccessExpressionProvider<TIdentifier, TEntity> identifierEntityAccessValidator
) : ITrackedReader<TIdentifier, TEntity>
where TIdentifier : struct
where TEntity : class, IEntity<TIdentifier>
{
    public async Task<TEntity?> FindByIdAsync(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken)
    {
        var filter = await CreateFilter(userContext);

        var specification = Specification
            .For<TEntity>()
            .WhereIdEquals(identifier)
            .Specification;

        return await repository.FindAsync(specification, filter, cancellationToken);
    }

    public async IAsyncEnumerable<TEntity> FindManyByIdAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var filter = await CreateFilter(userContext);

        var specification = Specification
            .For<TEntity>()
            .WhereIdIn(identifiers)
            .Specification;

        await foreach (var item in repository.FindManyAsync(specification, filter, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
        }
    }

    public async Task<IReadOnlyDictionary<TIdentifier, TEntity>> GetManyByIdAsync(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken)
    {
        var identifiersCollection = identifiers.AsCollection();

        var result = await FindManyByIdAsync(userContext, identifiersCollection, cancellationToken)
            .ToDictionaryAsync(e => e.Identifier, cancellationToken: cancellationToken);

        if (result.Count != identifiersCollection.Count)
            throw new EntityNotFoundException<TIdentifier, TEntity>(identifiersCollection.Except(result.Keys));

        return result;
    }

    public async IAsyncEnumerable<TEntity> GetAllAsync(UserContext userContext, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var filter = await CreateFilter(userContext);
        var specification = Specification.For<TEntity>().Specification;

        var enumerable = repository.FindManyAsync(specification, filter, cancellationToken);

        await foreach (var item in enumerable)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
        }
    }

    public async Task<TEntity?> FindAsync(UserContext userContext, Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var filter = await CreateFilter(userContext);
        var result = await repository.FindAsync(specification, filter, cancellationToken);

        return result;
    }

    public async IAsyncEnumerable<TEntity> FindManyAsync(UserContext userContext, Specification<TEntity> specification, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var filter = await CreateFilter(userContext);
        var result = repository.FindManyAsync(specification, filter, cancellationToken);

        await foreach (var item in result)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
        }
    }

    private async Task<Expression<Func<TEntity, bool>>> CreateFilter(UserContext userContext)
    {
        var access = await identifierEntityAccessValidator.GetReadExpression(userContext);
        var exists = DddExpressionFactory.Exists<TEntity>();
        
        var filter = DddExpressionUtils.And(exists, access);

        return filter;
    }
}
