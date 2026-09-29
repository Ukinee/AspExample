using System.Linq.Expressions;
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

public class FactoryReadEntityAccessExpressionProvider<TIdentifier, TEntity>(Func<UserContext, Expression<Func<TEntity, bool>>> factory)
    : IEntityReadAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public async Task<Expression<Func<TEntity, bool>>> GetReadExpression(UserContext userContext)
    {
        return factory(userContext);
    }
}

public class FactoryUpdateEntityAccessExpressionProvider<TIdentifier, TEntity>(Func<UserContext, Expression<Func<TEntity, bool>>> factory)
    : IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public async Task<Expression<Func<TEntity, bool>>> GetUpdateExpression(UserContext userContext)
    {
        return factory(userContext);
    }
}

public class FactoryDeleteEntityAccessExpressionProvider<TIdentifier, TEntity>(Func<UserContext, Expression<Func<TEntity, bool>>> factory)
    : IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public async Task<Expression<Func<TEntity, bool>>> GetDeleteExpression(UserContext userContext)
    {
        return factory(userContext);
    }
}

public class FactoryCreateEntityAccessExpressionProvider<TIdentifier, TEntity>(Func<UserContext, Expression<Func<TEntity, bool>>> factory)
    : IEntityCreateAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public async Task<Expression<Func<TEntity, bool>>> GetCreateExpression(UserContext userContext)
    {
        return factory(userContext);
    }
}
