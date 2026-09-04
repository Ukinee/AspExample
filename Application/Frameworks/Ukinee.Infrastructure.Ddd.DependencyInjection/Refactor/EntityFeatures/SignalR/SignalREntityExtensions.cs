using Microsoft.AspNetCore.SignalR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.SignalR;

// ReSharper disable once CheckNamespace
namespace Ukinee.AspNetCore.ModuleRegisterer.Refactor.EntityFeatures;

public class SignalRBuilderProxy<TTag, TIdentifier, TEntity, TDefinition>(TDefinition definition)
where TEntity : class, IEntity<TIdentifier>
where TDefinition : ModuleDefinition<TTag>.EntityDefinition<TIdentifier, TEntity>
where TIdentifier : notnull
{
    public ModuleDefinition<TTag>.EntityDefinition<TIdentifier, TEntity> Register<THub, TRequest, TViewModel>(
        Func<IGroupRoutingResolverRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>, SignalRFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>> factory
    )
    where THub : Hub
    {
        var feature = new SignalRDefinition<THub, TRequest, TViewModel> {
            RouteResolver = null!,
            AccessValidator = null!,
            Sink = null!,
        };

        var builder = new SignalRFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>(feature);
        builder.WithBatchedSink();

        factory.Invoke(builder);
        definition.Features.Add(builder.Feature);

        return definition;
    }
}

public static class SignalREntityExtensions
{
    //todo: uncomment whenever CS9295 is fixed in this context or write code generator

    // extension<TTag, TIdentifier, TEntity, TDefinition>(TDefinition definition)
    // where TIdentifier : struct, IEquatable<TIdentifier>
    // where TEntity : class, IEntity<TIdentifier>
    // where TDefinition : ModuleDefinition<TTag>.EntityDefinition<TIdentifier, TEntity>
    // {
    //     public SignalRBuilderProxy<TTag, TIdentifier, TEntity, TDefinition> SignalR =>
    //         new SignalRBuilderProxy<TTag, TIdentifier, TEntity, TDefinition>(definition);
    // }

    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public SignalRBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity>> SignalR =>
            new SignalRBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity>>(definition);
    }

    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public SignalRBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity>> SignalR =>
            new SignalRBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity>>(definition);
    }

    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>, ISpecificationForSoftDelete<TEntity>
    {
        public SignalRBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity>> SignalR =>
            new SignalRBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity>>(definition);
    }
}
