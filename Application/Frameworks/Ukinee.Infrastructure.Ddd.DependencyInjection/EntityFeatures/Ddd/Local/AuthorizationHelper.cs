using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Implementations;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Utils;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;

public static class AuthorizationHelper
{
    public static IEnumerable<ServiceDescriptor> Build<TIdentifier, TEntity>(AuthorizationPolicy<TIdentifier, TEntity> policy)
    where TEntity : class, IEntity<TIdentifier>
    {
        yield return CreateRead(policy.ReadLevel, policy);
        yield return CreateUpdate(policy.UpdateLevel, policy);
        yield return CreateCreate(policy.CreateLevel, policy);
        yield return CreateDelete(policy.DeleteLevel, policy);
    }

    private static ServiceDescriptor CreateDelete<TIdentifier, TEntity>(AccessLevel accessLevel, AuthorizationPolicy<TIdentifier, TEntity> policy)
    where TEntity : class, IEntity<TIdentifier>
    {
        if (accessLevel is AccessLevel.Guest or AccessLevel.LoggedIn)
            throw CreateOperationAccessNotSupported<TEntity>(accessLevel, AuthorizedOperation.Delete);

        var expressionFactory = CreateFactoryFor(AuthorizedOperation.Delete, accessLevel, policy);

        return ServiceDescriptor
            .Singleton<IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity>>(_ =>
                new FactoryDeleteEntityAccessExpressionProvider<TIdentifier, TEntity>(expressionFactory)
            );
    }

    private static ServiceDescriptor CreateCreate<TIdentifier, TEntity>(AccessLevel accessLevel, AuthorizationPolicy<TIdentifier, TEntity> policy)
    where TEntity : class, IEntity<TIdentifier>
    {
        if (accessLevel is AccessLevel.Guest or AccessLevel.Owner)
            throw CreateOperationAccessNotSupported<TEntity>(accessLevel, AuthorizedOperation.Create);

        var expressionFactory = CreateFactoryFor(AuthorizedOperation.Create, accessLevel, policy);

        return ServiceDescriptor
            .Singleton<IEntityCreateAccessExpressionProvider<TIdentifier, TEntity>>(_ =>
                new FactoryCreateEntityAccessExpressionProvider<TIdentifier, TEntity>(expressionFactory)
            );
    }

    private static ServiceDescriptor CreateUpdate<TIdentifier, TEntity>(AccessLevel accessLevel, AuthorizationPolicy<TIdentifier, TEntity> policy)
    where TEntity : class, IEntity<TIdentifier>
    {
        if (accessLevel is AccessLevel.Guest or AccessLevel.LoggedIn)
            throw CreateOperationAccessNotSupported<TEntity>(accessLevel, AuthorizedOperation.Update);

        var expressionFactory = CreateFactoryFor(AuthorizedOperation.Update, accessLevel, policy);

        return ServiceDescriptor
            .Singleton<IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity>>(_ =>
                new FactoryUpdateEntityAccessExpressionProvider<TIdentifier, TEntity>(expressionFactory)
            );
    }

    private static ServiceDescriptor CreateRead<TIdentifier, TEntity>(AccessLevel accessLevel, AuthorizationPolicy<TIdentifier, TEntity> policy)
    where TEntity : class, IEntity<TIdentifier>
    {
        if (accessLevel is AccessLevel.Guest or AccessLevel.LoggedIn)
            ThrowIfEntityIsNotForPublicRead<TIdentifier, TEntity>();

        var expressionFactory = CreateFactoryFor(AuthorizedOperation.Read, accessLevel, policy);

        return ServiceDescriptor
            .Singleton<IEntityReadAccessExpressionProvider<TIdentifier, TEntity>>(_ =>
                new FactoryReadEntityAccessExpressionProvider<TIdentifier, TEntity>(expressionFactory)
            );
    }

    private static Func<UserContext, Expression<Func<TEntity, bool>>> CreateFactoryFor<TIdentifier, TEntity>(
        AuthorizedOperation operation,
        AccessLevel level,
        AuthorizationPolicy<TIdentifier, TEntity> policy
    )
    where TEntity : IEntity<TIdentifier>
    {
        switch (level)
        {
            case AccessLevel.Guest: return DddAccessExpressionUtils.GuestExpression<TEntity>;
            case AccessLevel.LoggedIn: return DddAccessExpressionUtils.LoggedInExpression<TEntity>;
            case AccessLevel.Admin: return DddAccessExpressionUtils.AdminExpression<TEntity>;

            case AccessLevel.Owner:
                ThrowIfIdentifierExpressionIsNull(operation, policy);

                return DddAccessExpressionUtils.CreateOwnerUserGuidInIdentifierExpressionFactory<TIdentifier, TEntity>(policy.OwnerUserIdentifierInEntityIdentifierAccessor!);

            default: throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }

    private static void ThrowIfEntityIsNotForPublicRead<TIdentifier, TEntity>()
    where TEntity : IEntity<TIdentifier>
    {
        if (!typeof(IEntityWithPublicRead<TEntity>).IsAssignableFrom(typeof(TEntity)))
        {
            throw new InvalidOperationException($"{typeof(TEntity)} does not implement {typeof(IEntityWithPublicRead<TEntity>)} and cannot use Public Read policy.");
        }
    }

    [Pure]
    private static NotSupportedException CreateOperationAccessNotSupported<TEntity>(AccessLevel level, AuthorizedOperation operation)
    {
        return new NotSupportedException($"Cannot register {nameof(AuthorizedOperation)}.{operation} with {nameof(AccessLevel)}.{level} for {typeof(TEntity)}.");
    }

    private static void ThrowIfIdentifierExpressionIsNull<TIdentifier, TEntity>(AuthorizedOperation operation, AuthorizationPolicy<TIdentifier, TEntity> policy)
    where TEntity : IEntity<TIdentifier>
    {
        if (policy.OwnerUserIdentifierInEntityIdentifierAccessor is null)
        {
            throw new InvalidOperationException(
                $"Cannot register {operation} operation for {typeof(TEntity)}. Passed {nameof(policy)}.{nameof(policy.OwnerUserIdentifierInEntityIdentifierAccessor)} is null."
            );
        }
    }
}
