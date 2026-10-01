using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.Utils;

public static class DddExpressionFactory
{
    public const string ExpressionParameterName = "entity";

    sealed private class GuidBox
    {
        public Guid Value;
    }

    /// <summary>
    /// TEntity MUST contain expressionParameterName similar to TId expressionParameterName. This is achieved with [HasIdentifierAttribute]
    /// </summary>
    /// <param name="expressionParameterName"></param>
    /// <param name="propertyName"></param>
    /// <param name="userContext"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TEntity, bool>> UserIdentifierRule<TEntity>(
        string expressionParameterName,
        string propertyName,
        UserContext userContext
    )
    {
        var box = new GuidBox { Value = userContext.Guid };
        var capturedGuid = Expression.Field(Expression.Constant(box), nameof(GuidBox.Value));

        var parameter = Expression.Parameter(typeof(TEntity), expressionParameterName);
        var propertyAccess = Expression.Property(parameter, propertyName);
        var equality = Expression.Equal(propertyAccess, capturedGuid);

        return Expression.Lambda<Func<TEntity, bool>>(equality, parameter);
    }

    public static Expression<Func<TEntity, bool>> UserGuidInIdentifierExpression<TEntity>(string guidPropertyName, UserContext userContext) =>
        UserIdentifierRule<TEntity>(ExpressionParameterName, guidPropertyName, userContext);

    public static Expression<Func<TEntity, bool>> AdminExpression<TEntity>(UserContext userContext) =>
        _ => userContext.IsAdmin && userContext.IsAuthenticated;

    public static Expression<Func<TEntity, bool>> LoggedInExpression<TEntity>(UserContext userContext) =>
        _ => userContext.IsAuthenticated;

    public static Expression<Func<TEntity, bool>> GuestExpression<TEntity>(UserContext userContext) =>
        _ => true;

    public static Expression<Func<TEntity, bool>> True<TEntity>() =>
        _ => true;

    public static Expression<Func<TEntity, bool>> False<TEntity>() =>
        _ => false;

    public static bool IsPublicRead<TEntity>() =>
        PublicReadFilter<TEntity>.Applies;

    public static Expression<Func<TEntity, bool>> PublicRead<TEntity>() =>
        PublicReadFilter<TEntity>.Filter;

    public static bool IsSoftDelete<TEntity>() =>
        SoftDeleteFilter<TEntity>.Applies;

    public static Expression<Func<TEntity, bool>> Exists<TEntity>() =>
        SoftDeleteFilter<TEntity>.Filter;

    public static Expression<Func<TEntity, bool>> SharedWithUserExpression<TEntity>(UserContext userContext)
    where TEntity : IEntity
    {
        throw new NotImplementedException();
    }

    public static bool HasSharedAccess<TEntity>()
    where TEntity : IEntity
    {
        throw new NotImplementedException();
    }
}

internal static class SoftDeleteFilter<TEntity>
{
    public static readonly bool Applies = typeof(TEntity)
        .GetInterfaces()
        .Any(i => i.IsGenericType
                  && i.GetGenericTypeDefinition() == typeof(IEntityWithSoftDelete<>)
        );

    public static Expression<Func<TEntity, bool>> Filter => field ??= Applies ? CreateFilter() : _ => true;

    private static Expression<Func<TEntity, bool>> CreateFilter()
    {
        var p = Expression.Parameter(typeof(TEntity), "x");
        var prop = Expression.Property(p, nameof(IEntityWithSoftDelete<>.DeletedAt));
        var body = Expression.Equal(prop, Expression.Constant(null, typeof(DateTimeOffset?)));

        return Expression.Lambda<Func<TEntity, bool>>(body, p);
    }
}

internal static class PublicReadFilter<TEntity>
{
    public static readonly bool Applies = typeof(TEntity)
        .GetInterfaces()
        .Any(i => i.IsGenericType
                  && i.GetGenericTypeDefinition() == typeof(IEntityWithPublicRead<>)
        );

    public static Expression<Func<TEntity, bool>> Filter {
        get
        {
            if (!Applies)
            {
                throw new InvalidOperationException(
                    $"Cannot build public-read filter for {typeof(TEntity)}: " +
                    $"it does not implement {typeof(IEntityWithPublicRead<TEntity>).Name}."
                );
            }

            return field ??= CreateFilter();
        }
    }

    private static Expression<Func<TEntity, bool>> CreateFilter()
    {
        var p = Expression.Parameter(typeof(TEntity), "x");
        var prop = Expression.Property(p, nameof(IEntityWithPublicRead<>.IsAvailableForPublicRead));
        var body = Expression.Equal(prop, Expression.Constant(true));

        return Expression.Lambda<Func<TEntity, bool>>(body, p);
    }
}
