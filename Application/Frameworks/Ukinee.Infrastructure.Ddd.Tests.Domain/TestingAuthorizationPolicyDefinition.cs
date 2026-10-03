using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.Ddd.Tests.Domain;

public class TestingAuthorizationPolicyDefinition
{
    public const string Vip = "VIP";

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> GuestReadAndVipOwnerEdit<TIdentifier, TEntity>(
        Expression<Func<TIdentifier, Guid>> identifierUserGuid
    )
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateRules = [CreateAccessRule.LoggedIn(Vip)],
            ReadRules = [ReadAccessRule.Guest(), ReadAccessRule.OwnerOnly(Vip)],
            UpdateRules = [ModifyAccessRule.OwnerOnly(Vip)],
            DeleteRules = [ModifyAccessRule.OwnerOnly(Vip)],
            OwnerIdentifierAccessor = identifierUserGuid,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> LoggedInReadAndVipOwnerEdit<TIdentifier, TEntity>(
        Expression<Func<TIdentifier, Guid>> identifierUserGuid
    )
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateRules = [CreateAccessRule.LoggedIn(Vip)],
            ReadRules = [ReadAccessRule.LoggedIn(), ReadAccessRule.OwnerOnly(Vip)],
            UpdateRules = [ModifyAccessRule.OwnerOnly(Vip)],
            DeleteRules = [ModifyAccessRule.OwnerOnly(Vip)],
            OwnerIdentifierAccessor = identifierUserGuid,
        };

    public static AuthorizationPolicyDefinition<TIdentifier, TEntity> VipOwnerOnly<TIdentifier, TEntity>(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
    where TEntity : IEntity<TIdentifier> =>
        new AuthorizationPolicyDefinition<TIdentifier, TEntity> {
            CreateRules = [CreateAccessRule.LoggedIn(Vip)],
            ReadRules = [ReadAccessRule.OwnerOnly(Vip)],
            UpdateRules = [ModifyAccessRule.OwnerOnly(Vip)],
            DeleteRules = [ModifyAccessRule.OwnerOnly(Vip)],
            OwnerIdentifierAccessor = identifierUserGuid,
        };
}
