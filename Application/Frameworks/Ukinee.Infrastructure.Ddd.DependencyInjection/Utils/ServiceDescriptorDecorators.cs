using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;

public static class ServiceDescriptorDecorators
{
    public static void Decorate<TService, TDecorator>(List<ServiceDescriptor> descriptors)
    where TService : class
    where TDecorator : class, TService
    {
        var services = new ServiceCollection();

        foreach (var d in descriptors)
            services.Add(d);

        services.Decorate<TService, TDecorator>();

        descriptors.Clear();
        descriptors.AddRange(services);
    }
}
