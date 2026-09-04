using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;

public interface IBuildable
{
    public void Build(IReadOnlyCollection<Type> ignoredInterfaces, IServiceCollection serviceCollection);
}

public class ActionBuildable(Action<IServiceCollection> action) : IBuildable
{
    public void Build(IReadOnlyCollection<Type> ignoredInterfaces, IServiceCollection serviceCollection)
    {
        action(serviceCollection);
    }
}
