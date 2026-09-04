using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;

public class ModuleBuilder<TTag>(IServiceCollection serviceCollection)
{
    private readonly ModuleDefinition<TTag> _definition = new ModuleDefinition<TTag> {
        Contents = [],
    };

    public void AddAction(Action<IServiceCollection> action)
    {
        _definition.Contents.Add(new ActionBuildable(action));
    }

    public void Build(params IReadOnlyCollection<Type> ignoredInterfaces)
    {
        _definition.Build(ignoredInterfaces, serviceCollection);
    }
}
