using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;

public class LocalModuleDefinitionBuilder<TTag>(ModuleBuilder<TTag> builder)
{
    public ModuleBuilder<TTag> AddInMemoryContext<TIdentifier, TEntity>(Action<ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity>> contextConfigurator)
    {
        var definition = new ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> {
            Features = [],
        };

        contextConfigurator.Invoke(definition);
        builder.Definition.Contents.Add(definition);

        return builder;
    }

    public ModuleBuilder<TTag> AddSoftDeletionDatabaseContext<TIdentifier, TEntity>(Action<ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity>> contextConfigurator)
    where TEntity : IEntityWithSoftDelete<TEntity>
    {
        var definition = new ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> {
            Features = [],
        };

        contextConfigurator.Invoke(definition);
        builder.Definition.Contents.Add(definition);

        return builder;
    }

    public ModuleBuilder<TTag> AddHonestDeletionDatabaseContext<TIdentifier, TEntity>(Action<ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity>> contextConfigurator)
    {
        if (typeof(TEntity).IsAssignableTo(typeof(IEntityWithSoftDelete<TEntity>)))
            throw new InvalidOperationException($"Cannot add HonestDeletionEntityDefinition to {typeof(TEntity).Name}. Use {nameof(AddSoftDeletionDatabaseContext)} instead.");

        var definition = new ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> {
            Features = [],
        };

        contextConfigurator.Invoke(definition);
        builder.Definition.Contents.Add(definition);

        return builder;
    }
}

public static class LocalModuleBuilderExtensions
{
    extension<TTag>(ModuleBuilder<TTag> builder)
    {
        public LocalModuleDefinitionBuilder<TTag> LocalContexts => new LocalModuleDefinitionBuilder<TTag>(builder);
    }
}