using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.Utils;

public static class DddExpressionFactory
{
    /// <summary>
    /// TEntity MUST contain expressionParameterName similar to TId expressionParameterName. This is achieved with [HasIdentifierAttribute]
    /// </summary>
    /// <param name="expressionParameterName"></param>
    /// <param name="propertyName"></param>
    /// <param name="userContext"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TEntity, bool>> UserIdentifierRule<TEntity>(string expressionParameterName, string propertyName, UserContext userContext)
    {
        var userGuid = userContext.Guid;

        var parameter = Expression.Parameter(typeof(TEntity), expressionParameterName);
        var propertyAccess = Expression.Property(parameter, propertyName);
        var equality = Expression.Equal(propertyAccess, Expression.Constant(userGuid));

        return Expression.Lambda<Func<TEntity, bool>>(equality, parameter);
    }

    public static Expression<Func<TEntity, bool>> PublicRead<TEntity>()
    where TEntity : IEntityWithPublicRead<TEntity>
    {
        return ent => ent.IsAvailableForPublicRead;
    }

    public static Expression<Func<TEntity, bool>> Exists<TEntity>()
    {
        return SoftDeleteFilter<TEntity>.Filter;
    }
}

internal static class SoftDeleteFilter<TEntity>
{
    public static readonly bool Applies = typeof(IEntityWithSoftDelete<TEntity>).IsAssignableFrom(typeof(TEntity));

    public static Expression<Func<TEntity, bool>> Filter => field ??= Applies ? CreateFilter() : _ => true;

    private static Expression<Func<TEntity, bool>> CreateFilter()
    {
        var p = Expression.Parameter(typeof(TEntity), "x");
        var prop = Expression.Property(p, nameof(IEntityWithSoftDelete<>.DeletedAt));
        var body = Expression.Equal(prop, Expression.Constant(null, typeof(DateTimeOffset?)));

        return Expression.Lambda<Func<TEntity, bool>>(body, p);
    }
}
