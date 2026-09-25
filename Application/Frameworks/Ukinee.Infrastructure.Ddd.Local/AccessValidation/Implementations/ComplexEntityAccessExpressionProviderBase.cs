using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Utils;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation.Implementations;

public abstract class ComplexEntityAccessExpressionProviderBase<TIdentifier, TEntity> : IEntityAccessExpressionProvider<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    protected const string ExpressionParameterName = DddAccessExpressionUtils.ExpressionParameterName;

    public abstract Task<Expression<Func<TEntity, bool>>> GetCreateExpression(UserContext userContext);
    public abstract Task<Expression<Func<TEntity, bool>>> GetReadExpression(UserContext userContext);
    public abstract Task<Expression<Func<TEntity, bool>>> GetUpdateExpression(UserContext userContext);
    public abstract Task<Expression<Func<TEntity, bool>>> GetDeleteExpression(UserContext userContext);

#region Helpers
    public static Expression<Func<TEntity, bool>> Or(Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right) =>
        DddAccessExpressionUtils.Or(left, right);

    public static Expression<Func<TEntity, bool>> And(Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right) =>
        DddAccessExpressionUtils.And(left, right);

    public static Expression<Func<TEntity, bool>> UserGuidInIdentifierExpression(string guidPropertyName, UserContext userContext) =>
        DddAccessExpressionUtils.UserGuidInIdentifierExpression<TEntity>(guidPropertyName, userContext);

    public static Expression<Func<TEntity, bool>> AdminExpression(UserContext userContext) =>
        DddAccessExpressionUtils.AdminExpression<TEntity>(userContext);

    public static Expression<Func<TEntity, bool>> LoggedInExpression(UserContext userContext) =>
        DddAccessExpressionUtils.LoggedInExpression<TEntity>(userContext);

    public static Expression<Func<TEntity, bool>> GuestExpression(UserContext userContext) =>
        DddAccessExpressionUtils.GuestExpression<TEntity>(userContext);
#endregion
}
