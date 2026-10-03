using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;

public static class ApiAuthorizationHelper
{
    public static void ApplyPolicy<TIdentifier, TEntity>(
        RouteHandlerBuilder endpoint,
        AuthorizationPolicyDefinition<TIdentifier, TEntity> policyDefinition,
        AuthorizedOperation operation
    )
    where TEntity : class, IEntity<TIdentifier>
    {
        var requirement = policyDefinition.RequirementFor(operation);

        if (!requirement.RequiresAuth)
            return;

        if (requirement.AnyOfRoles.IsDefaultOrEmpty)
        {
            endpoint.RequireAuthorization(UserPolicies.LoggedInPolicy);

            return;
        }

        endpoint.RequireAuthorization(
            new AuthorizeAttribute {
                Roles = string.Join(",", requirement.AnyOfRoles),
            }
        );
    }
}

file static class AuthorizationEndpointExtensions
{
    public static EndpointAuthRequirement RequirementFor<TIdentifier, TEntity>(
        this AuthorizationPolicyDefinition<TIdentifier, TEntity> def,
        AuthorizedOperation operation
    )
    where TEntity : class, IEntity<TIdentifier> =>
        operation switch {
            AuthorizedOperation.Read => Combine(def.ReadRules.Select(r => (IsGuest: r.Mode == ReadAccessMode.Guest, r.RequiredRoles))),
            AuthorizedOperation.Create => Combine(def.CreateRules.Select(r => (IsGuest: false, r.RequiredRoles))),
            AuthorizedOperation.Update => Combine(def.UpdateRules.Select(r => (IsGuest: false, r.RequiredRoles))),
            AuthorizedOperation.Delete => Combine(def.DeleteRules.Select(r => (IsGuest: false, r.RequiredRoles))),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };

    private static EndpointAuthRequirement Combine(IEnumerable<(bool IsGuest, ImmutableArray<string> Roles)> rules)
    {
        var all = rules.ToList();

        if (all.Any(r => r.IsGuest))
            return new EndpointAuthRequirement(RequiresAuth: false, AnyOfRoles: ImmutableArray<string>.Empty);

        if (all.Any(r => r.Roles.IsDefaultOrEmpty))
            return new EndpointAuthRequirement(RequiresAuth: true, AnyOfRoles: ImmutableArray<string>.Empty);

        var anyOf = all.SelectMany(r => r.Roles).Distinct().ToImmutableArray();

        return new EndpointAuthRequirement(RequiresAuth: true, AnyOfRoles: anyOf);
    }
}

internal readonly record struct EndpointAuthRequirement(
    bool RequiresAuth,
    ImmutableArray<string> AnyOfRoles
);
