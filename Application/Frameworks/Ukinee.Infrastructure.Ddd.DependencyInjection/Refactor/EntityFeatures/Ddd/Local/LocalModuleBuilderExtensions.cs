using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Local;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Local
{
    public class LocalModuleDefinitionBuilder<TTag>(ModuleBuilder<TTag> builder)
    {
        public ModuleBuilder<TTag> AddInMemoryContext<TIdentifier, TEntity>(Action<ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity>> factory)
        {
            var definition = new ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> {
                Features = [],
            };

            factory.Invoke(definition);

            return builder;
        }

        public ModuleBuilder<TTag> AddSoftDeletionDatabaseContext<TIdentifier, TEntity>(Action<ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity>> factory)
        where TEntity : ISpecificationForSoftDelete<TEntity>
        {
            var definition = new ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> {
                Features = [],
            };

            factory.Invoke(definition);

            return builder;
        }

        public ModuleBuilder<TTag> AddHonestDeletionDatabaseContext<TIdentifier, TEntity>(Action<ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity>> factory)
        {
            var definition = new ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> {
                Features = [],
            };

            factory.Invoke(definition);

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
}

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core
{
    public partial record ModuleDefinition<TTag>
    {
        public record InMemoryEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>;
        public record HonestDeletionEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>;

        public record SoftDeletionEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>
        where TEntity : ISpecificationForSoftDelete<TEntity>;
    }
}
