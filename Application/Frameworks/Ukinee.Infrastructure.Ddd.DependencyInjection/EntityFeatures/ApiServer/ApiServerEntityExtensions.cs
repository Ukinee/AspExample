using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;

public class ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, TDefinition>(TDefinition definition)
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>
where TDefinition : ModuleDefinition<TTag>.EntityDefinition<TIdentifier, TEntity>
{
    public TDefinition Register<TParams, TResponse>(
        string baseRoute,
        Func<TParams, UserContext, TIdentifier> idFactory,
        AuthorizationPolicy<TIdentifier, TEntity> policy,
        Action<ICrudGroupConfigurator<TIdentifier, TEntity, TParams, TResponse>> apiServerConfigurator
    )
    where TParams : struct, IRouteParams<TParams, TIdentifier>
    {
        var feature = new ApiServerDefinition {
            BaseRoute = baseRoute,
            Tag = $"{typeof(TEntity).Name}Tag",
        };

        var builder = new ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse>(idFactory, policy, feature);

        apiServerConfigurator.Invoke(builder);

        definition.Features.Add(builder.Feature);

        return definition;
    }
}

public static class ApiEndpointEntityExtensions
{
    //todo: uncomment whenever CS9295 is fixed in this context or write code generator
    
    // extension<TTag, TIdentifier, TEntity, TDefinition>(TDefinition definition)
    // where TIdentifier : struct, IEquatable<TIdentifier>
    // where TEntity : class, IEntity<TIdentifier>
    // where TDefinition : ModuleDefinition<TTag>.EntityDefinition<TIdentifier, TEntity>
    // {
    //     public ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, TDefinition> ApiServer => new ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, TDefinition>(this);
    // }

    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity>> ApiServer =>
            new ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity>>(definition);
    }
    
    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity>> ApiServer =>
            new ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity>>(definition);
    }
    
    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>, IEntityWithSoftDelete<TEntity>
    {
        public ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity>> ApiServer =>
            new ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity>>(definition);
    }
}
