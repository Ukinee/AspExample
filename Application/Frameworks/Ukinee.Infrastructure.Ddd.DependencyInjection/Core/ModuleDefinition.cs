using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Core;

public partial record ModuleDefinition<TTag> : IBuildable
{
    internal List<IBuildable> Contents { get; init; } = [];

    public abstract record EntityDefinition<TIdentifier, TEntity> : IBuildable
    {
        internal List<Action<IServiceCollection>> Services { get; init; } = [];
        internal List<IBuildable> Features { get; init; } = [];

        public void Build(RegistrationPolicy policy, IServiceCollection serviceCollection)
        {
            foreach (var action in Services)
                action(serviceCollection);

            foreach (var feature in Features)
                feature.Build(policy, serviceCollection);
        }
    }

    public void Build(RegistrationPolicy policy, IServiceCollection serviceCollection)
    {
        foreach (var entity in Contents)
            entity.Build(policy, serviceCollection);
    }
}
