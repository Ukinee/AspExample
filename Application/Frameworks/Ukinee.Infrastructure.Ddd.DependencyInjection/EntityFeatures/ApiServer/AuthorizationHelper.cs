using Microsoft.AspNetCore.Builder;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;

public static class AuthorizationHelper
{
    public static void ApplyPolicy<TIdentifier, TEntity>(RouteHandlerBuilder endpoint, AuthorizationPolicyDefinition<TIdentifier, TEntity> policyDefinition, AuthorizedOperation operation)
    where TEntity : class, IEntity<TIdentifier>
    {
        ApplyPolicy(endpoint, policyDefinition, policyDefinition.AccessLevelFor(operation));
    }

    public static void ApplyPolicy<TIdentifier, TEntity>(RouteHandlerBuilder endpoint, AuthorizationPolicyDefinition<TIdentifier, TEntity> policyDefinition, AccessLevel accessLevel)
    where TEntity : class, IEntity<TIdentifier>
    {
        switch (accessLevel)
        {
            case AccessLevel.Guest: break;
            case AccessLevel.LoggedIn:
            case AccessLevel.Owner: endpoint.RequireAuthorization(UserPolicies.LoggedInPolicy); break;
            case AccessLevel.Admin: endpoint.RequireAuthorization(UserPolicies.AdminPolicy); break;
            default: throw new ArgumentOutOfRangeException(nameof(accessLevel), accessLevel, null);
        }
    }
}
