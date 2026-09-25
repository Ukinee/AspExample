using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;

public static class TrackedDddBuilderExtensions
{
    extension<TTag, TIdentifier, TEntity>(ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity> builder)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct
    {
        // public TrackedDddBuilder<TTag, TIdentifier, TEntity> LoggedInReadAndAdministratorEdit()
        // {
        //     var policy = AuthorizationPolicies.LoggedInReadAndAdministratorEdit<TIdentifier, TEntity>();
        //
        //     return builder.SetAccessPolicy(policy);
        // }
        //
        // public TrackedDddBuilder<TTag, TIdentifier, TEntity> GuestReadAndAdministratorEdit()
        // {
        //     var policy = AuthorizationPolicies.GuestReadAndAdministratorEdit<TIdentifier, TEntity>();
        //
        //     return builder.SetAccessPolicy(policy);
        // }
        //
        // public TrackedDddBuilder<TTag, TIdentifier, TEntity> OwnerOnly(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
        // {
        //     var policy = AuthorizationPolicies.OwnerOnly<TIdentifier, TEntity>(identifierUserGuid);
        //
        //     return builder.SetAccessPolicy(policy);
        // }
        //
        // public TrackedDddBuilder<TTag, TIdentifier, TEntity> GuestReadAndOwnerEdit(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
        // {
        //     var policy = AuthorizationPolicies.GuestReadAndOwnerEdit<TIdentifier, TEntity>(identifierUserGuid);
        //
        //     return builder.SetAccessPolicy(policy);
        // }
        //
        // public TrackedDddBuilder<TTag, TIdentifier, TEntity> LoggedInReadAndOwnerEdit(Expression<Func<TIdentifier, Guid>> identifierUserGuid)
        // {
        //     var policy = AuthorizationPolicies.LoggedInReadAndOwnerEdit<TIdentifier, TEntity>(identifierUserGuid);
        //
        //     return builder.SetAccessPolicy(policy);
        // }
    }
}
