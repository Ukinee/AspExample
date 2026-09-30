using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation.Implementations;

public class AggregatingEntityAccessExpressionProvider<TIdentifier, TEntity>(
    IEntityReadAccessExpressionProvider<TIdentifier, TEntity> readAccessExpression,
    IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity> updateAccessExpression,
    IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity> deleteAccessExpression,
    IEntityCreateAccessExpressionProvider<TIdentifier, TEntity> createAccessExpression
) : IEntityAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public Task<Expression<Func<TEntity, bool>>> GetCreateExpression(UserContext userContext)
    {
        return createAccessExpression.GetCreateExpression(userContext);
    }

    public Task<Expression<Func<TEntity, bool>>> GetReadExpression(UserContext userContext)
    {
        return readAccessExpression.GetReadExpression(userContext);
    }

    public Task<Expression<Func<TEntity, bool>>> GetUpdateExpression(UserContext userContext)
    {
        return updateAccessExpression.GetUpdateExpression(userContext);
    }

    public Task<Expression<Func<TEntity, bool>>> GetDeleteExpression(UserContext userContext)
    {
        return deleteAccessExpression.GetDeleteExpression(userContext);
    }
}

public class PolicyReadEntityAccessExpressionProvider<TIdentifier, TEntity>(AuthorizationPolicy<TIdentifier, TEntity> policyDefinition)
    : IEntityReadAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public async Task<Expression<Func<TEntity, bool>>> GetReadExpression(UserContext userContext)
    {
        return policyDefinition.ReadFactory.Invoke(userContext);
    }
}

public class PolicyUpdateEntityAccessExpressionProvider<TIdentifier, TEntity>(AuthorizationPolicy<TIdentifier, TEntity> policyDefinition)
    : IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public async Task<Expression<Func<TEntity, bool>>> GetUpdateExpression(UserContext userContext)
    {
        return policyDefinition.UpdateFactory.Invoke(userContext);
    }
}

public class PolicyDeleteEntityAccessExpressionProvider<TIdentifier, TEntity>(AuthorizationPolicy<TIdentifier, TEntity> policyDefinition)
    : IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public async Task<Expression<Func<TEntity, bool>>> GetDeleteExpression(UserContext userContext)
    {
        return policyDefinition.DeleteFactory.Invoke(userContext);
    }
}

public class PolicyCreateEntityAccessExpressionProvider<TIdentifier, TEntity>(AuthorizationPolicy<TIdentifier, TEntity> policyDefinition)
    : IEntityCreateAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public async Task<Expression<Func<TEntity, bool>>> GetCreateExpression(UserContext userContext)
    {
        return policyDefinition.CreateFactory.Invoke(userContext);
    }
}
