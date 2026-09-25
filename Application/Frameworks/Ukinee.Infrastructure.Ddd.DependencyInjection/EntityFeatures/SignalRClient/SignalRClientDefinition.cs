using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRClient;

public class SignalRClientDefinition<TSubscribeRequest, TViewModel> : IBuildable
{
    public List<ServiceDescriptor> HubConnectionRegisterer { get; } = [];
    public List<ServiceDescriptor> CacheRequestHandlers { get; } = [];

    public void Build(RegistrationPolicy policy, IServiceCollection serviceCollection)
    {
        PayloadHelper.AddFiltered(serviceCollection, policy, HubConnectionRegisterer);
        PayloadHelper.AddFiltered(serviceCollection, policy, CacheRequestHandlers);
    }
}
