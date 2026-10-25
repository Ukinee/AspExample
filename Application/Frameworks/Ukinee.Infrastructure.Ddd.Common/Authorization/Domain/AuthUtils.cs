using System.Collections.Immutable;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;

public static class AuthUtils
{
    public static string? Canonicalize(ImmutableArray<string> roles) =>
        roles.IsDefaultOrEmpty ? null : string.Join("&&", roles.OrderBy(x => x, StringComparer.Ordinal));

    public static bool HasAllRoles(UserContext ctx, ImmutableArray<string> required) =>
        required.IsDefaultOrEmpty || Enumerable.All(required, role => ctx.Roles.Contains(role));

    public static IEnumerable<AccessTag> ToTags(UserContext userContext, SubscribeAccessModes modes, ImmutableArray<ReadAccessRule> rules)
    {
        foreach (var rule in rules)
        {
            if (!MatchesFlag(rule.Mode, modes))
                continue;

            if (rule.Mode is not ReadAccessMode.Guest && !userContext.IsAuthenticated)
                continue;

            if (!HasAllRoles(userContext, rule.RequiredRoles))
                continue;

            var rolesKey = Canonicalize(rule.RequiredRoles);

            yield return rule.Mode switch {
                ReadAccessMode.Guest or ReadAccessMode.LoggedIn => new AccessTag.Public(),
                ReadAccessMode.OwnerOnly => new AccessTag.Owner(userContext.Guid),
                ReadAccessMode.SharedUsers => new AccessTag.Shared(userContext.Guid),
                ReadAccessMode.Roles => new AccessTag.Roles(rolesKey!),
                _ => throw new ArgumentOutOfRangeException(nameof(rule.Mode)),
            };
        }
    }

    private static bool MatchesFlag(ReadAccessMode ruleMode, SubscribeAccessModes modes)
    {
        return ruleMode switch {
            ReadAccessMode.Guest => modes.HasFlagFast(SubscribeAccessModes.Guest),
            ReadAccessMode.LoggedIn => modes.HasFlagFast(SubscribeAccessModes.LoggedIn),
            ReadAccessMode.OwnerOnly => modes.HasFlagFast(SubscribeAccessModes.OwnerOnly),
            ReadAccessMode.SharedUsers => modes.HasFlagFast(SubscribeAccessModes.SharedUsers),
            ReadAccessMode.Roles => modes.HasFlagFast(SubscribeAccessModes.Roles),
            _ => throw new ArgumentOutOfRangeException(nameof(ruleMode), ruleMode, null)
        };
    }
}
