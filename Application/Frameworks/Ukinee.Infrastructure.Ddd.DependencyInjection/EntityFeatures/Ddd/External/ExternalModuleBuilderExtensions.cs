using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;

public class ExternalModuleDefinitionBuilder<TTag>(ModuleBuilder<TTag> builder)
{
    public ModuleBuilder<TTag> AddRemoteContext<TIdentifier, TEntity>(Action<ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity>> factory)
    {
        var definition = new ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> {
            Features = [],
        };

        factory.Invoke(definition);
        builder.Definition.Contents.Add(definition);

        return builder;
    }
}

public static class ExternalModuleBuilderExtensions
{
    extension<TTag>(ModuleBuilder<TTag> builder)
    {
        public ExternalModuleDefinitionBuilder<TTag> ApiClientContexts => new ExternalModuleDefinitionBuilder<TTag>(builder);
    }
}