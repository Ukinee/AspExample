using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.SignalR;

public class SignalRDefinition<THub, TRequest, TViewModel> : IBuildable
{
    public class RouteResolverDefinition
    {
        public required Type Type { get; init; }
    }

    public class AccessValidatorDefinition
    {
        public required Type Type { get; init; }
    }

    public class SinkDefinition
    {
        public required Type Type { get; init; }
    }
    
    private List<Action<IServiceCollection>> _services = [];

    public required RouteResolverDefinition RouteResolver { get; set; }
    public required AccessValidatorDefinition AccessValidator { get; set; }
    public required SinkDefinition Sink { get; set; }

    public void AddServices(Action<IServiceCollection> services)
    {
        _services.Add(services);
    }

    public void Build(IReadOnlyCollection<Type> allowedInterfaces, IServiceCollection serviceCollection)
    {
        foreach (var service in _services)
            service.Invoke(serviceCollection);

        List<ServiceDescriptor> result = [];
        
        result.AddRange(PayloadHelper.GetSingletonDescriptors(RouteResolver.Type, allowedInterfaces));
        result.AddRange(PayloadHelper.GetSingletonDescriptors(AccessValidator.Type, allowedInterfaces));
        result.AddRange(PayloadHelper.GetSingletonDescriptors(Sink.Type, allowedInterfaces));

        foreach (var descriptor in result)
            serviceCollection.TryAdd(descriptor);
    }
}
