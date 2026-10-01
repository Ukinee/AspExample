using System.Collections.Immutable;
using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;

public enum ReadAccessMode
{
    Guest,
    LoggedIn,
    OwnerOnly,
    SharedUsers,
    Roles
}

public enum CreateAccessMode
{
    LoggedIn,
    Roles
}

public enum ModifyAccessMode
{
    OwnerOnly,
    SharedUsers,
    Roles
}

public readonly record struct ReadAccessRule
{
    public required ReadAccessMode Mode { get; init; }
    public ImmutableArray<string> RequiredRoles { get; init; }

    public static ReadAccessRule Guest() =>
        new() { Mode = ReadAccessMode.Guest };

    public static ReadAccessRule LoggedIn(params ImmutableArray<string> r) =>
        new() { Mode = ReadAccessMode.LoggedIn, RequiredRoles = r };

    public static ReadAccessRule OwnerOnly(params ImmutableArray<string> r) =>
        new() { Mode = ReadAccessMode.OwnerOnly, RequiredRoles = r };

    public static ReadAccessRule SharedUsers(params ImmutableArray<string> r) =>
        new() { Mode = ReadAccessMode.SharedUsers, RequiredRoles = r };

    public static ReadAccessRule Roles(params ImmutableArray<string> r) =>
        new() { Mode = ReadAccessMode.Roles, RequiredRoles = r };
}

public readonly record struct CreateAccessRule
{
    public required CreateAccessMode Mode { get; init; }
    public ImmutableArray<string> RequiredRoles { get; init; }

    public static CreateAccessRule LoggedIn(params ImmutableArray<string> r) =>
        new() { Mode = CreateAccessMode.LoggedIn, RequiredRoles = r };

    public static CreateAccessRule Roles(params ImmutableArray<string> r) =>
        new() { Mode = CreateAccessMode.Roles, RequiredRoles = r };
}

public readonly record struct ModifyAccessRule
{
    public required ModifyAccessMode Mode { get; init; }
    public ImmutableArray<string> RequiredRoles { get; init; }

    public static ModifyAccessRule OwnerOnly(params ImmutableArray<string> r) =>
        new() { Mode = ModifyAccessMode.OwnerOnly, RequiredRoles = r };

    public static ModifyAccessRule SharedUsers(params ImmutableArray<string> r) =>
        new() { Mode = ModifyAccessMode.SharedUsers, RequiredRoles = r };

    public static ModifyAccessRule Roles(params ImmutableArray<string> r) =>
        new() { Mode = ModifyAccessMode.Roles, RequiredRoles = r };
}

public sealed record AuthorizationPolicyDefinition<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
{
    public required ImmutableArray<CreateAccessRule> CreateRules { get; init; }
    public required ImmutableArray<ReadAccessRule> ReadRules { get; init; }
    public required ImmutableArray<ModifyAccessRule> UpdateRules { get; init; }
    public required ImmutableArray<ModifyAccessRule> DeleteRules { get; init; }

    public Expression<Func<TIdentifier, Guid>>? OwnerIdentifierAccessor { get; init; }
}

public static class AuthorizationPolicyDefinition
{
    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> LoggedInReadAndAdministratorEdit<TIdentifier, TEntity>()
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateRules = [CreateAccessRule.Roles(UserRoles.Admin)],
            ReadRules = [ReadAccessRule.LoggedIn(), ReadAccessRule.Roles(UserRoles.Admin)],
            UpdateRules = [ModifyAccessRule.Roles(UserRoles.Admin)],
            DeleteRules = [ModifyAccessRule.Roles(UserRoles.Admin)],
            OwnerIdentifierAccessor = null,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> GuestReadAndAdministratorEdit<TIdentifier, TEntity>()
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateRules = [CreateAccessRule.Roles(UserRoles.Admin)],
            ReadRules = [ReadAccessRule.Guest(), ReadAccessRule.Roles(UserRoles.Admin)],
            UpdateRules = [ModifyAccessRule.Roles(UserRoles.Admin)],
            DeleteRules = [ModifyAccessRule.Roles(UserRoles.Admin)],
            OwnerIdentifierAccessor = null,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> AdministratorOnly<TIdentifier, TEntity>()
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateRules = [CreateAccessRule.Roles(UserRoles.Admin)],
            ReadRules = [ReadAccessRule.Roles(UserRoles.Admin)],
            UpdateRules = [ModifyAccessRule.Roles(UserRoles.Admin)],
            DeleteRules = [ModifyAccessRule.Roles(UserRoles.Admin)],
            OwnerIdentifierAccessor = null,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> OwnerOnly<TIdentifier, TEntity>(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateRules = [CreateAccessRule.LoggedIn()],
            ReadRules = [ReadAccessRule.OwnerOnly()],
            UpdateRules = [ModifyAccessRule.OwnerOnly()],
            DeleteRules = [ModifyAccessRule.OwnerOnly()],
            OwnerIdentifierAccessor = identifierUserGuid,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> GuestReadAndOwnerEdit<TIdentifier, TEntity>(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateRules = [CreateAccessRule.LoggedIn()],
            ReadRules = [ReadAccessRule.Guest(), ReadAccessRule.OwnerOnly()],
            UpdateRules = [ModifyAccessRule.OwnerOnly()],
            DeleteRules = [ModifyAccessRule.OwnerOnly()],
            OwnerIdentifierAccessor = identifierUserGuid,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> LoggedInReadAndOwnerEdit<TIdentifier, TEntity>(
        Expression<Func<TIdentifier, Guid>> identifierUserGuid
    )
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateRules = [CreateAccessRule.LoggedIn()],
            ReadRules = [ReadAccessRule.LoggedIn(), ReadAccessRule.OwnerOnly()],
            UpdateRules = [ModifyAccessRule.OwnerOnly()],
            DeleteRules = [ModifyAccessRule.OwnerOnly()],
            OwnerIdentifierAccessor = identifierUserGuid,
        };
}
