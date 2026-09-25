using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Core;

public interface IBuildable
{
    public void Build(RegistrationPolicy policy, IServiceCollection serviceCollection);
}

public class ActionBuildable(Action<IServiceCollection> action) : IBuildable
{
    public void Build(RegistrationPolicy policy, IServiceCollection serviceCollection)
    {
        action(serviceCollection);
    }
}

public sealed class RegistrationPolicy
{
    private readonly HashSet<Type> _ignored = new();
    private readonly HashSet<Type> _multiple = new();

    public RegistrationPolicy Ignore<T>()
    {
        _ignored.Add(typeof(T));

        return this;
    }

    public RegistrationPolicy AllowMultiple<T>()
    {
        _multiple.Add(typeof(T));

        return this;
    }

    public bool IsIgnored(Type t) =>
        _ignored.Contains(t);

    public bool AllowsMultiple(Type t) =>
        _multiple.Contains(t);
}
