using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;

public static class PayloadHelper
{
    public static List<ServiceDescriptor> GetSingletonDescriptors(Type type, IReadOnlyCollection<Type> ignoredInterfaces)
    {
        var typeDescriptor = ServiceDescriptor.Singleton(type, type);

        return SingletonDescriptors(type, typeDescriptor, ignoredInterfaces);
    }
    
    public static List<ServiceDescriptor> GetSingletonDescriptors(IEnumerable<Type> types, IReadOnlyCollection<Type> ignoredInterfaces)
    {
        var result = new List<ServiceDescriptor>();

        foreach (var type in types)
        {
            result.AddRange(GetSingletonDescriptors(type, ignoredInterfaces));
        }
        
        return result;
    }

    public static List<ServiceDescriptor> GetSingletonDescriptors(object instance, IReadOnlyCollection<Type> ignoredInterfaces)
    {
        var type = instance.GetType();
        var typeDescriptor = ServiceDescriptor.Singleton(type, instance);

        return SingletonDescriptors(type, typeDescriptor, ignoredInterfaces);
    }

    private static List<ServiceDescriptor> SingletonDescriptors(Type type, ServiceDescriptor typeDescriptor, IReadOnlyCollection<Type> ignoredInterfaces)
    {
        List<ServiceDescriptor> result = [typeDescriptor];

        var interfaces = type.GetInterfaces();

        foreach (var @interface in interfaces)
        {
            if (IsIgnored(ignoredInterfaces, @interface))
                continue;

            var descriptor = ServiceDescriptor.Singleton(@interface, sp => sp.GetRequiredService(type));
            result.Add(descriptor);
        }

        return result;
    }

    private static bool IsIgnored(IReadOnlyCollection<Type> ignoredInterfaces, Type interfaceType)
    {
        if (ignoredInterfaces.Contains(interfaceType))
            return true;

        if (!interfaceType.IsGenericType)
            return false;

        var genericDefinition = interfaceType.GetGenericTypeDefinition();

        return ignoredInterfaces.Contains(genericDefinition);
    }
}
