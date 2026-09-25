using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;

public static class ServiceDescriptorDecorators
{
    public static void Decorate<TService>(
        List<ServiceDescriptor> descriptors,
        Func<Type, Type> closeDecorator
    )
    where TService : class
    {
        var index = descriptors.FindIndex(d => d.ServiceType == typeof(TService));

        if (index < 0)
            throw new InvalidOperationException($"Cannot decorate: {typeof(TService).Name} is not registered.");

        var current = descriptors[index];

        var innerType = current.ImplementationType ?? TryFindConcreteSibling<TService>(descriptors, current);

        if (innerType is null)
            throw new NotSupportedException(
                $"Cannot auto-detect inner for factory-based registration of {typeof(TService).Name}. " +
                $"Use Decorate<TService, TInner, TDecorator> explicitly."
            );

        var lifetime = current.Lifetime;

        var decoratorType = closeDecorator(innerType);

        if (!typeof(TService).IsAssignableFrom(decoratorType))
            throw new InvalidOperationException($"Decorator {decoratorType.Name} does not implement {typeof(TService).Name}.");

        if (!decoratorType.IsClass || decoratorType.IsAbstract)
            throw new InvalidOperationException($"Decorator {decoratorType.Name} must be a concrete class.");

        EnsureConcrete(descriptors, innerType, lifetime);
        EnsureConcrete(descriptors, decoratorType, lifetime);

        descriptors[index] = new ServiceDescriptor(typeof(TService), sp => sp.GetRequiredService(decoratorType), lifetime);
    }

    private static Type? TryFindConcreteSibling<TService>(
        List<ServiceDescriptor> descriptors,
        ServiceDescriptor current
    )
    where TService : class
    {
        return descriptors
            .Where(d => !ReferenceEquals(d, current))
            .Select(d => d.ServiceType)
            .Where(t => t != typeof(TService))
            .Where(t => typeof(TService).IsAssignableFrom(t))
            .Where(t => t.IsClass && !t.IsAbstract)
            .Distinct()
            .FirstOrDefault();
    }

    private static void EnsureConcrete(List<ServiceDescriptor> descriptors, Type type, ServiceLifetime lifetime)
    {
        if (descriptors.Any(d => d.ServiceType == type))
            return;

        descriptors.Add(DescriptorFor(type, type, lifetime));
    }

    private static ServiceDescriptor DescriptorFor(Type serviceType, Type implType, ServiceLifetime lifetime)
    {
        return new ServiceDescriptor(serviceType, implType, lifetime);
    }
}
