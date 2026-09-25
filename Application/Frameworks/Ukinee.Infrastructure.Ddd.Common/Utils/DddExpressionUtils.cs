using System.Linq.Expressions;
using LinqKit;

namespace Ukinee.Infrastructure.Ddd.Common.Utils;

public static class DddExpressionUtils
{
    public static Expression<Func<TEntity, bool>> And<TEntity>(Expression<Func<TEntity, bool>> lhs, Expression<Func<TEntity, bool>> rhs)
    {
        var expressionStarter = PredicateBuilder.New(lhs);

        return expressionStarter.And(rhs).Expand();
    }

    public static Expression<Func<TEntity, bool>> Or<TEntity>(Expression<Func<TEntity, bool>> lhs, Expression<Func<TEntity, bool>> rhs)
    {
        var expressionStarter = PredicateBuilder.New(lhs);

        return expressionStarter.Or(rhs).Expand();
    }

    public static string GetPropertyName(Expression expression)
    {
        if (expression is ParameterExpression parameterExpression)
        {
            return parameterExpression.Name!;
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

    public static Expression<Func<TEntity, bool>> CombineAnd<TEntity>(IReadOnlyCollection<Expression<Func<TEntity, bool>>?> collection)
    {
        if (collection.Count == 0)
        {
            return entity => false;
        }

        ExpressionStarter<TEntity> expressionStarter = PredicateBuilder.New<TEntity>(true);
        bool hasActiveRules = false;

        foreach (Expression<Func<TEntity, bool>>? expression in collection)
        {
            if (expression == null)
                continue;

            expressionStarter = expressionStarter.And(expression);
            hasActiveRules = true;
        }

        if (!hasActiveRules)
            return entity => false;

        Expression<Func<TEntity, bool>> typedExpression = expressionStarter;

        return typedExpression.Expand();
    }
}
