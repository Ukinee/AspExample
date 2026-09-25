using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Utils;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation.Implementations;

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
