using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Utils;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation.Utils;

public class DddAccessExpressionUtils
{
    public const string ExpressionParameterName = DddExpressionFactory.ExpressionParameterName;

    public static Expression<Func<TEntity, bool>> Or<TEntity>(Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right) =>
        DddExpressionUtils.Or(left, right);

    public static Expression<Func<TEntity, bool>> And<TEntity>(Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right) =>
        DddExpressionUtils.And(left, right);

    public static Expression<Func<TEntity, bool>> UserGuidInIdentifierExpression<TEntity>(string guidPropertyName, UserContext userContext) =>
        DddExpressionFactory.UserGuidInIdentifierExpression<TEntity>(guidPropertyName, userContext);

    public static Expression<Func<TEntity, bool>> PublicRead<TEntity>() =>
        DddExpressionFactory.PublicRead<TEntity>();

    public static Expression<Func<TEntity, bool>> Exists<TEntity>() =>
        DddExpressionFactory.Exists<TEntity>();
}
