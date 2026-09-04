using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Exceptions;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation_Rethink.Implementations;

public class IdentifierEntityAccessValidator<TIdentifier, TEntity>(
    Expression<Func<TIdentifier, Guid>> identifierExpression,
    IAccessViolationReporter<TIdentifier, TEntity> reporter
) : IIdentifierEntityAccessValidator<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct
{
    private readonly Func<TIdentifier, Guid> _compiledIdentifier = identifierExpression.Compile();

    private readonly string _targetPropertyName = GetPropertyName(identifierExpression.Body);

    public bool HasAccess(UserContext userContext, TIdentifier identifier)
    {
        if (userContext.IsAdmin)
            return true;

        var hasAccess = _compiledIdentifier(identifier) == userContext.Guid;

        if (!hasAccess)
            reporter.ReportViolation(userContext, [identifier]);

        return hasAccess;
    }

    public (IReadOnlyCollection<TIdentifier> AllowedAccess, IReadOnlyCollection<TIdentifier> DeniedAccess) Separate(UserContext userContext, IEnumerable<TIdentifier> identifiers)
    {
        if (userContext.IsAdmin)
            return (identifiers.ToList(), []);

        var lookup = identifiers.ToLookup(identifier => _compiledIdentifier(identifier) == userContext.Guid);

        return (lookup[true].ToList(), lookup[false].ToList());
    }

    public void EnsureHasAccess(UserContext userContext, IEnumerable<TEntity> entities)
    {
        if (userContext.IsAdmin)
            return;

        var userGuid = userContext.Guid;

        var violatedEntities = entities.Where(entity => _compiledIdentifier(entity.Identifier) != userGuid).ToList();

        if (violatedEntities.Count == 0)
            return;

        reporter.ReportViolation(userContext, violatedEntities);

        throw new EntityAccessDeniedException<TIdentifier, TEntity>(userContext, violatedEntities.Select(e => e.Identifier).ToList());
    }

    public Expression<Func<TEntity, bool>> GetExpression(UserContext userContext)
    {
        if (userContext.IsAdmin)
            return ent => true;

        var userGuid = userContext.Guid;

        var parameter = Expression.Parameter(typeof(TEntity), "ent");

        var propertyAccess = Expression.Property(parameter, _targetPropertyName);

        var equality = Expression.Equal(propertyAccess, Expression.Constant(userGuid));

        return Expression.Lambda<Func<TEntity, bool>>(equality, parameter);
    }

    public void EnsureHasAccess(UserContext userContext, IEnumerable<TIdentifier> identifiers)
    {
        var userGuid = userContext.Guid;

        var violatedIdentifiers = identifiers.Where(identifier => _compiledIdentifier(identifier) != userGuid).ToList();

        if (violatedIdentifiers.Count == 0)
            return;

        reporter.ReportViolation(userContext, violatedIdentifiers);

        throw new EntityAccessDeniedException<TIdentifier, TEntity>(userContext, violatedIdentifiers);
    }

    private static string GetPropertyName(Expression expression)
    {
        if (expression is ParameterExpression)
        {
            return nameof(IEntity<>.Identifier);
        }

        if (expression is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
        {
            return GetPropertyName(unaryExpression.Operand);
        }

        throw new InvalidOperationException("Expression must be either id => id (for Guid), either struct property (id => id.UserGuid).");
    }
}
