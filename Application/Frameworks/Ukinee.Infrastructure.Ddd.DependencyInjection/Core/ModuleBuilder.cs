using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Core;

public class ModuleBuilder<TTag>(IServiceCollection serviceCollection)
{
    internal readonly ModuleDefinition<TTag> Definition = new ModuleDefinition<TTag> {
        Contents = [],
    };

    public void AddAction(Action<IServiceCollection> action)
    {
        Definition.Contents.Add(new ActionBuildable(action));
    }

    public void Build(RegistrationPolicy policy)
    {
        Definition.Build(policy, serviceCollection);
    }
}
