using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Utils;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Local.AccessValidation.Utils;

public class DddAccessExpressionUtils
{
    public const string ExpressionParameterName = "entity";

    public static Expression<Func<TEntity, bool>> Or<TEntity>(Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right) =>
        DddExpressionUtils.Or(left, right);

    public static Expression<Func<TEntity, bool>> And<TEntity>(Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right) =>
        DddExpressionUtils.And(left, right);

    public static Expression<Func<TEntity, bool>> UserGuidInIdentifierExpression<TEntity>(string guidPropertyName, UserContext userContext) =>
        DddExpressionFactory.UserIdentifierRule<TEntity>(ExpressionParameterName, guidPropertyName, userContext);

    public static Expression<Func<TEntity, bool>> AdminExpression<TEntity>(UserContext userContext) =>
        _ => userContext.IsAdmin && userContext.IsAuthenticated;

    public static Expression<Func<TEntity, bool>> LoggedInExpression<TEntity>(UserContext userContext) =>
        _ => userContext.IsAuthenticated;

    public static Expression<Func<TEntity, bool>> GuestExpression<TEntity>(UserContext userContext) =>
        _ => true;

    public static Func<UserContext, Expression<Func<TEntity, bool>>> CreateOwnerUserGuidInIdentifierExpressionFactory<TIdentifier, TEntity>(
        Expression<Func<TIdentifier, Guid>> userIdentifierAccessor
    )
    where TEntity : IEntity<TIdentifier>
    {
        var propertyName = DddExpressionUtils.GetPropertyName(userIdentifierAccessor);

        return context => UserGuidInIdentifierExpression<TEntity>(propertyName, context);
    }
}
