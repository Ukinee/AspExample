using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.ApiEndpoints;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.EntityFeatures.ApiEndpoints;

public class ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, TDefinition>(TDefinition definition)
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>
where TDefinition : ModuleDefinition<TTag>.EntityDefinition<TIdentifier, TEntity>
{
    public TDefinition Register<TParams, TResponse>(
        string baseRoute,
        Func<TParams, UserContext, TIdentifier> idFactory,
        Action<ICrudGroupConfigurator<TIdentifier, TEntity, TParams, TResponse>> factory
    )
    where TParams : struct, IRouteParams<TParams, TIdentifier>
    {
        var feature = new ApiServerDefinition {
            BaseRoute = $"{baseRoute}",
            Tag = $"{typeof(TEntity).Name}Tag",
        };

        var builder = new ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse>(idFactory, feature);

        factory.Invoke(builder);

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
    where TEntity : class, IEntity<TIdentifier>, ISpecificationForSoftDelete<TEntity>
    {
        public ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity>> ApiServer =>
            new ApiServerFeatureBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity>>(definition);
    }
}
