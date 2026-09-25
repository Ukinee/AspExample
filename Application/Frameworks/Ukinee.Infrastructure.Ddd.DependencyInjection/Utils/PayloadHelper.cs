using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;


public static class PayloadHelper
{
    public static IEnumerable<ServiceDescriptor> Singleton<TInterface, TImplementation>()
    where TInterface : class
    where TImplementation : class, TInterface
    {
        yield return ServiceDescriptor.Singleton<TImplementation, TImplementation>();
        yield return ServiceDescriptor.Singleton<TInterface, TImplementation>(sc => sc.GetRequiredService<TImplementation>());
    }

    public static void AddFiltered(
        IServiceCollection services,
        RegistrationPolicy policy,
        IEnumerable<ServiceDescriptor> descriptors
    )
    {
        foreach (var descriptor in descriptors)
        {
            var serviceType = descriptor.ServiceType;

            if (policy.IsIgnored(serviceType))
                continue;

            if (serviceType == descriptor.ImplementationType)
            {
                if (!services.Any(d => d.ServiceType == serviceType))
                    services.Add(descriptor);

                continue;
            }

            if (policy.AllowsMultiple(serviceType))
            {
                services.Add(descriptor);

                continue;
            }

            var existing = services.FirstOrDefault(d => d.ServiceType == serviceType);

            if (existing is not null)
            {
                throw new InvalidOperationException(
                    $"Service '{serviceType}' is registered more than once.\n" +
                    $"  existing: {Describe(existing)}\n" +
                    $"  new:      {Describe(descriptor)}"
                );
            }

            services.Add(descriptor);
        }
    }

    private static string Describe(ServiceDescriptor d) =>
        $"{d.ServiceType.Name} -> " + (d.ImplementationType?.Name ?? (d.ImplementationFactory is not null ? "<factory>" : "<instance>"));
}
