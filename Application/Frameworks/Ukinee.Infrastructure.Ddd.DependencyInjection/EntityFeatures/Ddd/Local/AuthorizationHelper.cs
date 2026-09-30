using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Implementations;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;

public static class AuthorizationHelper
{
    public static IEnumerable<ServiceDescriptor> GetDescriptors<TIdentifier, TEntity>(AuthorizationPolicyDefinition<TIdentifier, TEntity> policyDefinition)
    where TEntity : class, IEntity<TIdentifier>
    {
        var policy = AuthorizationPolicy.Compile(policyDefinition);

        yield return ServiceDescriptor.Singleton(policy);

        yield return ServiceDescriptor.Singleton<IEntityReadAccessExpressionProvider<TIdentifier, TEntity>,
            PolicyReadEntityAccessExpressionProvider<TIdentifier, TEntity>>();

        yield return ServiceDescriptor.Singleton<IEntityCreateAccessExpressionProvider<TIdentifier, TEntity>,
            PolicyCreateEntityAccessExpressionProvider<TIdentifier, TEntity>>();

        yield return ServiceDescriptor.Singleton<IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity>,
            PolicyUpdateEntityAccessExpressionProvider<TIdentifier, TEntity>>();

        yield return ServiceDescriptor.Singleton<IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity>,
            PolicyDeleteEntityAccessExpressionProvider<TIdentifier, TEntity>>();

        yield return ServiceDescriptor.Singleton<IEntityAccessExpressionProvider<TIdentifier, TEntity>,
            AggregatingEntityAccessExpressionProvider<TIdentifier, TEntity>>();
    }
}
