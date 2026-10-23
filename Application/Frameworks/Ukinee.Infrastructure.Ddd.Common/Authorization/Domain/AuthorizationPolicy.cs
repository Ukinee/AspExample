using System.Collections.Immutable;
using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Infrastructure.Ddd.Common.Utils;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;

public abstract record AccessTag
{
    public sealed record Public() : AccessTag
    {
        public override string ToGroupKey(string prefix)
        {
            return $"{prefix}.Public";
        }
    }

    public sealed record LoggedIn() : AccessTag
    {
        public override string ToGroupKey(string prefix)
        {
            return $"{prefix}.LoggedIn";
        }
    }

    public sealed record Owner(Guid OwnerGuid) : AccessTag
    {
        public override string ToGroupKey(string prefix)
        {
            return $"{prefix}.Owner.{OwnerGuid}";
        }
    }

    public sealed record Shared(Guid UserGuid) : AccessTag
    {
        public override string ToGroupKey(string prefix)
        {
            return $"{prefix}.Shared.{UserGuid}";
        }
    }

    public sealed record Roles(string CanonicalRolesKey) : AccessTag
    {
        public override string ToGroupKey(string prefix)
        {
            return $"{prefix}.Roles.{CanonicalRolesKey}";
        }
    }

    public abstract string ToGroupKey(string prefix);
}

public sealed record AuthorizationPolicy<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required Expression<Func<TEntity, bool>> ExistsExpression { get; init; }
    public required string? OwnerUserIdentifierPropertyName { get; init; }

    public required Func<TIdentifier, Guid>? OwnerUserIdentifierAccessor { get; init; }

    public required Func<UserContext, Expression<Func<TEntity, bool>>> ReadFactory { get; init; }
    public required Func<UserContext, Expression<Func<TEntity, bool>>> CreateFactory { get; init; }
    public required Func<UserContext, Expression<Func<TEntity, bool>>> UpdateFactory { get; init; }
    public required Func<UserContext, Expression<Func<TEntity, bool>>> DeleteFactory { get; init; }

    public required Func<TEntity, IReadOnlyCollection<AccessTag>> EntityTagsFactory { get; init; }
    public required Func<UserContext, SubscribeAccessModes, IEnumerable<AccessTag>> UserTagsFactory { get; init; }
}

public static class AuthorizationPolicy
{
    public static AuthorizationPolicy<TIdentifier, TEntity> Compile<TIdentifier, TEntity>(AuthorizationPolicyDefinition<TIdentifier, TEntity> definition)
    where TEntity : IEntity<TIdentifier>
    {
        Validate(definition);

        var ownerPropertyName = definition.OwnerIdentifierAccessor is not null
            ? DddExpressionUtils.GetPropertyName(definition.OwnerIdentifierAccessor)
            : null;

        var exists = DddExpressionFactory.NotSoftDeleted<TEntity>();

        var ownerUserIdentifierAccessor = definition.OwnerIdentifierAccessor?.Compile();

        return new AuthorizationPolicy<TIdentifier, TEntity> {
            ExistsExpression = exists,
            OwnerUserIdentifierPropertyName = ownerPropertyName,
            OwnerUserIdentifierAccessor = ownerUserIdentifierAccessor,
            ReadFactory = BuildReadFactory(definition.ReadRules, ownerPropertyName, exists),
            CreateFactory = BuildCreateFactory<TEntity>(definition.CreateRules),
            UpdateFactory = BuildMutateFactory(definition.UpdateRules, ownerPropertyName, exists),
            DeleteFactory = BuildMutateFactory(definition.DeleteRules, ownerPropertyName, exists),
            EntityTagsFactory = BuildEntityTagsFactory<TIdentifier, TEntity>(definition.ReadRules, ownerUserIdentifierAccessor),
            UserTagsFactory = BuildUserTagsFactory(definition),
        };
    }

    private static Func<UserContext, SubscribeAccessModes, IEnumerable<AccessTag>> BuildUserTagsFactory<TIdentifier, TEntity>(AuthorizationPolicyDefinition<TIdentifier, TEntity> definition)
    where TEntity : IEntity<TIdentifier>
    {
        return (userContext, modes) => AuthUtils.ToTags(userContext, modes, definition.ReadRules);
    }

    private static Func<TEntity, IReadOnlyCollection<AccessTag>> BuildEntityTagsFactory<TIdentifier, TEntity>(
        ImmutableArray<ReadAccessRule> definitionReadRules,
        Func<TIdentifier, Guid>? ownerUserIdentifierAccessor
    )
    where TEntity : IEntity<TIdentifier>
    {
        return entity =>
        {
            var tags = new List<AccessTag>();

            var loggedInEntity = entity as IEntityWithPublicRead<TEntity>;
            var sharedAccessEntity = entity as IEntityWithSharedAccess<TEntity>;

            foreach (var rule in definitionReadRules)
            {
                var rolesKey = AuthUtils.Canonicalize(rule.RequiredRoles);

                switch (rule.Mode)
                {
                    case ReadAccessMode.Guest or ReadAccessMode.LoggedIn:
                        if (loggedInEntity!.IsAvailableForPublicRead)
                            tags.Add(new AccessTag.Public());

                        break;

                    case ReadAccessMode.OwnerOnly: tags.Add(new AccessTag.Owner(ownerUserIdentifierAccessor!(entity.Identifier))); break;
                    case ReadAccessMode.SharedUsers: tags.AddRange(sharedAccessEntity!.SharedUserGuids.Select(guid => new AccessTag.Shared(guid))); break;
                    case ReadAccessMode.Roles: tags.Add(new AccessTag.Roles(rolesKey!)); break;

                    default: throw new ArgumentOutOfRangeException(nameof(rule.Mode));
                }
            }

            return tags;
        };
    }

    private static Func<UserContext, Expression<Func<TEntity, bool>>> BuildReadFactory<TEntity>(
        ImmutableArray<ReadAccessRule> rules,
        string? ownerPropertyName,
        Expression<Func<TEntity, bool>> exists
    )
    where TEntity : IEntity
    {
        return userContext =>
        {
            Expression<Func<TEntity, bool>>? combined = null;

            foreach (var rule in rules)
            {
                if (!AuthUtils.HasAllRoles(userContext, rule.RequiredRoles))
                    continue;

                var expr = rule.Mode switch {
                    ReadAccessMode.Guest => DddExpressionFactory.PublicRead<TEntity>(),
                    ReadAccessMode.LoggedIn => userContext.IsAuthenticated ? DddExpressionFactory.PublicRead<TEntity>() : null,
                    ReadAccessMode.OwnerOnly => DddExpressionFactory.UserGuidInIdentifierExpression<TEntity>(ownerPropertyName!, userContext),
                    ReadAccessMode.SharedUsers => DddExpressionFactory.SharedWithUserExpression<TEntity>(userContext),
                    ReadAccessMode.Roles => DddExpressionFactory.LoggedInExpression<TEntity>(userContext),
                    _ => throw new ArgumentOutOfRangeException(nameof(rule.Mode)),
                };

                if (expr is null)
                    continue;

                combined = combined is null ? expr : DddExpressionUtils.Or(combined, expr);
            }

            combined ??= DddExpressionFactory.False<TEntity>();

            return DddExpressionUtils.And(combined, exists);
        };
    }

    private static Func<UserContext, Expression<Func<TEntity, bool>>> BuildCreateFactory<TEntity>(ImmutableArray<CreateAccessRule> rules)
    where TEntity : IEntity
    {
        return ctx =>
        {
            var hasAccess = rules
                .Where(rule => AuthUtils.HasAllRoles(ctx, rule.RequiredRoles))
                .Any(rule => rule.Mode switch {
                        CreateAccessMode.LoggedIn => ctx.IsAuthenticated,
                        CreateAccessMode.Roles => ctx.IsAuthenticated,
                        _ => throw new ArgumentOutOfRangeException(nameof(rule.Mode)),
                    }
                );

            return hasAccess
                ? DddExpressionFactory.True<TEntity>()
                : DddExpressionFactory.False<TEntity>();
        };
    }

    private static Func<UserContext, Expression<Func<TEntity, bool>>> BuildMutateFactory<TEntity>(
        ImmutableArray<ModifyAccessRule> rules,
        string? ownerPropertyName,
        Expression<Func<TEntity, bool>> exists
    )
    where TEntity : IEntity
    {
        return ctx =>
        {
            Expression<Func<TEntity, bool>>? combined = null;

            foreach (var rule in rules)
            {
                if (!AuthUtils.HasAllRoles(ctx, rule.RequiredRoles))
                    continue;

                var expr = rule.Mode switch {
                    ModifyAccessMode.OwnerOnly => DddExpressionFactory.UserGuidInIdentifierExpression<TEntity>(ownerPropertyName!, ctx),
                    ModifyAccessMode.SharedUsers => DddExpressionFactory.SharedWithUserExpression<TEntity>(ctx),
                    ModifyAccessMode.Roles => DddExpressionFactory.LoggedInExpression<TEntity>(ctx),
                    _ => throw new ArgumentOutOfRangeException(nameof(rule.Mode)),
                };

                combined = combined is null ? expr : DddExpressionUtils.Or(combined, expr);
            }

            combined ??= DddExpressionFactory.False<TEntity>();

            return DddExpressionUtils.And(combined, exists);
        };
    }

    private static void Validate<TIdentifier, TEntity>(AuthorizationPolicyDefinition<TIdentifier, TEntity> def)
    where TEntity : IEntity<TIdentifier>
    {
        if (def.ReadRules.IsDefaultOrEmpty)
            throw new InvalidOperationException($"ReadRules is empty for {typeof(TEntity)}.");

        if (def.CreateRules.IsDefaultOrEmpty)
            throw new InvalidOperationException($"CreateRules is empty for {typeof(TEntity)}.");

        if (def.UpdateRules.IsDefaultOrEmpty)
            throw new InvalidOperationException($"UpdateRules is empty for {typeof(TEntity)}.");

        if (def.DeleteRules.IsDefaultOrEmpty)
            throw new InvalidOperationException($"DeleteRules is empty for {typeof(TEntity)}.");

        var needsPublicRead = def.ReadRules.Any(r => r.Mode is ReadAccessMode.Guest or ReadAccessMode.LoggedIn);

        if (needsPublicRead && !DddExpressionFactory.IsPublicRead<TEntity>())
            throw new InvalidOperationException(
                $"{typeof(TEntity)} does not implement {nameof(IEntityWithPublicRead<>)} (required by {ReadAccessMode.Guest}/{ReadAccessMode.LoggedIn} read rules)."
            );

        var needsShared = def.ReadRules.Any(r => r.Mode == ReadAccessMode.SharedUsers) ||
                          def.UpdateRules.Any(r => r.Mode == ModifyAccessMode.SharedUsers) ||
                          def.DeleteRules.Any(r => r.Mode == ModifyAccessMode.SharedUsers);

        if (needsShared && !DddExpressionFactory.HasSharedAccess<TEntity>())
            throw new InvalidOperationException(
                $"{typeof(TEntity)} does not implement {nameof(IEntityWithSharedAccess<>)} (required by {ReadAccessMode.SharedUsers} rules)."
            );

        var needsOwner = def.ReadRules.Any(r => r.Mode == ReadAccessMode.OwnerOnly) ||
                         def.UpdateRules.Any(r => r.Mode == ModifyAccessMode.OwnerOnly) ||
                         def.DeleteRules.Any(r => r.Mode == ModifyAccessMode.OwnerOnly);

        if (needsOwner && def.OwnerIdentifierAccessor is null)
            throw new InvalidOperationException($"OwnerOnly rules require {nameof(def.OwnerIdentifierAccessor)} for {typeof(TEntity)}.");

        foreach (var rule in def.ReadRules)
        {
            if (rule is { Mode: ReadAccessMode.Guest, RequiredRoles.IsDefaultOrEmpty: false })
                throw new InvalidOperationException("Guest read rule cannot have RequiredRoles.");

            if (rule is { Mode: ReadAccessMode.Roles, RequiredRoles.IsDefaultOrEmpty: true })
                throw new InvalidOperationException("Roles read rule must specify at least one role.");
        }

        foreach (var rule in def.CreateRules)
            if (rule is { Mode: CreateAccessMode.Roles, RequiredRoles.IsDefaultOrEmpty: true })
                throw new InvalidOperationException("Roles create rule must specify at least one role.");

        foreach (var rule in def.UpdateRules.Concat(def.DeleteRules))
            if (rule is { Mode: ModifyAccessMode.Roles, RequiredRoles.IsDefaultOrEmpty: true })
                throw new InvalidOperationException("Roles modify rule must specify at least one role.");
    }
}
