using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRServer;

public class SignalRServerDefinition<THub, TSubscribeRequest, TViewModel> : IBuildable
{
    private List<Action<IServiceCollection>> _services = [];

    public List<ServiceDescriptor> RouteResolver { get; } = [];
    public List<ServiceDescriptor> AccessValidator { get; } = [];
    public List<ServiceDescriptor> Sink { get; } = [];

    public void AddServices(Action<IServiceCollection> services)
    {
        _services.Add(services);
    }

    public void Build(RegistrationPolicy policy, IServiceCollection serviceCollection)
    {
        foreach (var service in _services)
            service.Invoke(serviceCollection);

        PayloadHelper.AddFiltered(serviceCollection, policy, RouteResolver);
        PayloadHelper.AddFiltered(serviceCollection, policy, AccessValidator);
        PayloadHelper.AddFiltered(serviceCollection, policy, Sink);
    }
}
