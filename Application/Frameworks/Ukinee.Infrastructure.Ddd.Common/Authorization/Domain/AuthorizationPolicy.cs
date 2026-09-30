using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Common.Utils;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;

public sealed record AuthorizationPolicy<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required AccessLevel CreateLevel { get; init; }
    public required AccessLevel ReadLevel { get; init; }
    public required AccessLevel UpdateLevel { get; init; }
    public required AccessLevel DeleteLevel { get; init; }

    public required Expression<Func<TEntity, bool>> ExistsExpression { get; init; }
    public required Expression<Func<TEntity, bool>>? PublicReadExpression { get; init; }
    public required string? OwnerUserIdentifierPropertyName { get; init; }

    public required Func<UserContext, Expression<Func<TEntity, bool>>> ReadFactory { get; init; }
    public required Func<UserContext, Expression<Func<TEntity, bool>>> CreateFactory { get; init; }
    public required Func<UserContext, Expression<Func<TEntity, bool>>> UpdateFactory { get; init; }
    public required Func<UserContext, Expression<Func<TEntity, bool>>> DeleteFactory { get; init; }
}

public static class AuthorizationPolicy
{
    public static AuthorizationPolicy<TIdentifier, TEntity> Compile<TIdentifier, TEntity>(AuthorizationPolicyDefinition<TIdentifier, TEntity> definition)
    where TEntity : IEntity<TIdentifier>
    {
        Validate(definition);

        var ownerPropertyName = definition.OwnerUserIdentifierInEntityIdentifierAccessor is not null
            ? DddExpressionUtils.GetPropertyName(definition.OwnerUserIdentifierInEntityIdentifierAccessor)
            : null;

        var exists = DddExpressionFactory.Exists<TEntity>();

        var publicRead = typeof(IEntityWithPublicRead<TEntity>).IsAssignableFrom(typeof(TEntity))
            ? DddExpressionFactory.PublicRead<TEntity>()
            : null;

        var readFactory = BuildReadFactory(ownerPropertyName, definition.ReadLevel, publicRead, exists);
        var createFactory = BuildCreateFactory<TEntity>(ownerPropertyName, definition.CreateLevel);
        var updateFactory = BuildMutateFactory(ownerPropertyName, definition.UpdateLevel, exists);
        var deleteFactory = BuildMutateFactory(ownerPropertyName, definition.DeleteLevel, exists);

        return new AuthorizationPolicy<TIdentifier, TEntity> {
            CreateLevel = definition.CreateLevel,
            ReadLevel = definition.ReadLevel,
            UpdateLevel = definition.UpdateLevel,
            DeleteLevel = definition.DeleteLevel,
            ExistsExpression = exists,
            PublicReadExpression = publicRead,
            OwnerUserIdentifierPropertyName = ownerPropertyName,
            ReadFactory = readFactory,
            CreateFactory = createFactory,
            UpdateFactory = updateFactory,
            DeleteFactory = deleteFactory,
        };
    }

    private static Func<UserContext, Expression<Func<TEntity, bool>>> BuildReadFactory<TEntity>(
        string? ownerPropertyName,
        AccessLevel accessLevel,
        Expression<Func<TEntity, bool>>? publicRead,
        Expression<Func<TEntity, bool>> exists
    )
    where TEntity : IEntity
    {
        var baseFactory = BuildBaseFactory<TEntity>(accessLevel, ownerPropertyName);

        if (accessLevel is not (AccessLevel.Guest or AccessLevel.LoggedIn))
            return ctx => DddExpressionUtils.And(baseFactory(ctx), exists);

        Func<UserContext, Expression<Func<TEntity, bool>>> publicOrOwner = ownerPropertyName is not null
            ? ctx => DddExpressionUtils.Or(publicRead!, DddExpressionFactory.UserGuidInIdentifierExpression<TEntity>(ownerPropertyName, ctx))
            : _ => publicRead!;

        return ctx => DddExpressionUtils.And(DddExpressionUtils.And(baseFactory(ctx), publicOrOwner(ctx)), exists);
    }

    private static Func<UserContext, Expression<Func<TEntity, bool>>> BuildCreateFactory<TEntity>(string? ownerPropertyName, AccessLevel accessLevel)
    where TEntity : IEntity =>
        BuildBaseFactory<TEntity>(accessLevel, ownerPropertyName);

    private static Func<UserContext, Expression<Func<TEntity, bool>>> BuildMutateFactory<TEntity>(
        string? ownerPropertyName,
        AccessLevel accessLevel,
        Expression<Func<TEntity, bool>> exists
    )
    where TEntity : IEntity
    {
        var baseFactory = BuildBaseFactory<TEntity>(accessLevel, ownerPropertyName);

        return ctx => DddExpressionUtils.And(baseFactory(ctx), exists);
    }

    private static Func<UserContext, Expression<Func<TEntity, bool>>> BuildBaseFactory<TEntity>(AccessLevel accessLevel, string? ownerPropertyName) =>
        accessLevel switch {
            AccessLevel.Guest => DddExpressionFactory.GuestExpression<TEntity>,
            AccessLevel.LoggedIn => DddExpressionFactory.LoggedInExpression<TEntity>,
            AccessLevel.Admin => DddExpressionFactory.AdminExpression<TEntity>,
            AccessLevel.Owner => ctx => DddExpressionFactory.UserGuidInIdentifierExpression<TEntity>(ownerPropertyName!, ctx),
            _ => throw new ArgumentOutOfRangeException(nameof(accessLevel)),
        };

    private static void Validate<TIdentifier, TEntity>(AuthorizationPolicyDefinition<TIdentifier, TEntity> def)
    where TEntity : IEntity<TIdentifier>
    {
        if (def.ReadLevel is AccessLevel.Guest or AccessLevel.LoggedIn && !DddExpressionFactory.IsPublicRead<TEntity>())
            throw new InvalidOperationException($"{typeof(TEntity)} does not implement {typeof(IEntityWithPublicRead<TEntity>)} and cannot use Public Read policy.");

        if (def.CreateLevel is AccessLevel.Guest or AccessLevel.Owner)
            throw new NotSupportedException($"Cannot register Create with {def.CreateLevel} for {typeof(TEntity)}.");

        if (def.UpdateLevel is AccessLevel.Guest or AccessLevel.LoggedIn)
            throw new NotSupportedException($"Cannot register Update with {def.UpdateLevel} for {typeof(TEntity)}.");

        if (def.DeleteLevel is AccessLevel.Guest or AccessLevel.LoggedIn)
            throw new NotSupportedException($"Cannot register Delete with {def.DeleteLevel} for {typeof(TEntity)}.");

        var needsOwner = def.ReadLevel == AccessLevel.Owner || def.UpdateLevel == AccessLevel.Owner || def.DeleteLevel == AccessLevel.Owner;
        
        if (needsOwner && def.OwnerUserIdentifierInEntityIdentifierAccessor is null)
            throw new InvalidOperationException($"Owner access level requires {nameof(def.OwnerUserIdentifierInEntityIdentifierAccessor)} for {typeof(TEntity)}.");
    }
}
