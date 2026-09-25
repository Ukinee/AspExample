using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRClient;

public class SignalRClientBuilderProxy<TTag, TIdentifier, TEntity, TDefinition>(TDefinition definition)
where TEntity : class, IEntity<TIdentifier>
where TDefinition : ModuleDefinition<TTag>.EntityDefinition<TIdentifier, TEntity>
where TIdentifier : struct, IEquatable<TIdentifier>
{
    public ModuleDefinition<TTag>.EntityDefinition<TIdentifier, TEntity> Register<THubTag, TSubscribeRequest, TViewModel>(
        Func<SignalRClientFeatureBuilder<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel>,
            SignalRClientFeatureBuilder<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel>> factory
    )
    {
        var feature = new SignalRClientDefinition<TSubscribeRequest, TViewModel>();

        var builder = new SignalRClientFeatureBuilder<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel>(feature);

        factory.Invoke(builder);

        definition.Features.Add(builder.Feature);

        return definition;
    }
}

public static class SignalRClientEntityExtensions
{
    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public SignalRClientBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity>> SignalRClient =>
            new SignalRClientBuilderProxy<TTag, TIdentifier, TEntity, ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity>>(definition);
    }
}
