using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;
using Ukinee.Infrastructure.SignalR.Client.Contracts;

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

    public static RegistrationPolicy Client => new RegistrationPolicy()
        .Ignore<IDisposable>()
        .AllowMultiple(typeof(IHubConnectionRegisterer<>));
    
    public static RegistrationPolicy Server => new RegistrationPolicy()
        .Ignore<IDisposable>()
        .AllowMultiple<ApiServerDefinition>();

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

    public RegistrationPolicy Ignore(Type type)
    {
        _ignored.Add(type);

        return this;
    }

    public RegistrationPolicy AllowMultiple(Type type)
    {
        _multiple.Add(type);

        return this;
    }

    public bool IsIgnored(Type t) =>
        Matches(_ignored, t);

    public bool AllowsMultiple(Type t) =>
        Matches(_multiple, t);

    private static bool Matches(HashSet<Type> set, Type t)
    {
        if (set.Contains(t))
            return true;

        return t.IsGenericType && set.Contains(t.GetGenericTypeDefinition());
    }
}
