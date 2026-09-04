using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;

public partial record ModuleDefinition<TTag> : IBuildable
{
    internal List<IBuildable> Contents { get; init; } = [];

    public abstract record EntityDefinition<TIdentifier, TEntity> : IBuildable
    {
        internal List<Action<IServiceCollection>> Services { get; init; } = [];
        internal List<IBuildable> Features { get; init; } = [];

        public void Build(IReadOnlyCollection<Type> allowedInterfaces, IServiceCollection serviceCollection)
        {
            foreach (var action in Services)
                action(serviceCollection);

            foreach (var feature in Features)
                feature.Build(allowedInterfaces, serviceCollection);
        }
    }

    public void Build(IReadOnlyCollection<Type> ignoredInterfaces, IServiceCollection serviceCollection)
    {
        foreach (var entity in Contents)
            entity.Build(ignoredInterfaces, serviceCollection);
    }
}
