using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Local;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.External
{
    public class ExternalModuleDefinitionBuilder<TTag>(ModuleBuilder<TTag> builder)
    {
        public ModuleBuilder<TTag> AddRemoteContext<TIdentifier, TEntity>(Action<ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity>> factory)
        {
            var definition = new ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> {
                Features = [],
            };

            factory.Invoke(definition);

            return builder;
        }
    }

    public static class ExternalModuleBuilderExtensions
    {
        extension<TTag>(ModuleBuilder<TTag> builder)
        {
            public LocalModuleDefinitionBuilder<TTag> ApiClientContexts => new LocalModuleDefinitionBuilder<TTag>(builder);
        }
    }
}

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core
{
    public partial record ModuleDefinition<TTag>
    {
        public record RemoteEntityDefinition<TIdentifier, TEntity> : EntityDefinition<TIdentifier, TEntity>;
    }
}
