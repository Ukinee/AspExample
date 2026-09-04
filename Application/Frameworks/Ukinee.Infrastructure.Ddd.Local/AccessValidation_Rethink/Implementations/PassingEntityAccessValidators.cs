using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Implementations;

public class PassingIdentifierEntityAccessValidator<TIdentifier, TEntity> : IIdentifierEntityAccessValidator<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public PassingIdentifierEntityAccessValidator(ILogger<PassingIdentifierEntityAccessValidator<TIdentifier, TEntity>> logger)
    {
        logger.LogWarning("Passing entity access validator is being used for {EntityType}.", typeof(TIdentifier).Name);
    }

    public Expression<Func<TEntity, bool>> GetExpression(UserContext userContext) =>
        entity => true;

    public bool HasAccess(UserContext userContext, TIdentifier identifier) =>
        true;

    public IEnumerable<TIdentifier> FilterAndReport(UserContext userContext, IEnumerable<TIdentifier> identifiers) =>
        identifiers;

    public void EnsureHasAccess(UserContext userContext, IEnumerable<TIdentifier> enumerable) { }

    public (IReadOnlyCollection<TIdentifier> AllowedAccess, IReadOnlyCollection<TIdentifier> DeniedAccess) Separate(UserContext userContext, IEnumerable<TIdentifier> identifiers)
    {
        var identifiersAsCollection = identifiers as IReadOnlyCollection<TIdentifier> ?? identifiers.ToList();

        return (identifiersAsCollection, []);
    }
}
