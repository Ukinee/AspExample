using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Services;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRServer;

public class SignalRServerFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> :
    IGroupRoutingResolverRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>,
    IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>
where TEntity : class, IEntity<TIdentifier>
where THub : Hub
{
    internal readonly SignalRServerDefinition<THub, TRequest, TViewModel> Feature;

    public SignalRServerFeatureBuilder(SignalRServerDefinition<THub, TRequest, TViewModel> feature)
    {
        Feature = feature;

        Feature.AddServices(sc => sc.AddSingleton<IHubService<TRequest>, HubService<TIdentifier, TEntity, TRequest>>());
        Feature.AddServices(sc => sc.AddSingleton<INotificationHandler<CreatedDomainEvent<TIdentifier, TEntity>>, SignalRDomainEventReceiver<TIdentifier, TEntity>>());
        Feature.AddServices(sc => sc.AddSingleton<INotificationHandler<UpdatedDomainEvent<TIdentifier, TEntity>>, SignalRDomainEventReceiver<TIdentifier, TEntity>>());
        Feature.AddServices(sc => sc.AddSingleton<INotificationHandler<DeletedDomainEvent<TIdentifier, TEntity>>, SignalRDomainEventReceiver<TIdentifier, TEntity>>());
    }

    IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>
        IGroupRoutingResolverRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>.SetGroupResolver<TImplementation>()
    {
        Feature.RouteResolver.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [..PayloadHelper.Singleton<IRouteResolver<TIdentifier, TEntity, TRequest>, TImplementation>(),];

        Feature.RouteResolver.AddRange(descriptors);

        return this;
    }

    SignalRServerFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>
        IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>.SetAccessValidationService<TImplementation>()
    {
        Feature.AccessValidator.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [..PayloadHelper.Singleton<ISignalRAccessValidator<TRequest>, TImplementation>(),];

        Feature.AccessValidator.AddRange(descriptors);

        return this;
    }

    internal void SetSignalRSink<TImplementation>()
    where TImplementation : class, ISignalRSink<TIdentifier, TEntity>
    {
        Feature.Sink.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [..PayloadHelper.Singleton<ISignalRSink<TIdentifier, TEntity>, TImplementation>(),];

        Feature.Sink.AddRange(descriptors);
    }
}

public interface IGroupRoutingResolverRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>
where TEntity : class, IEntity<TIdentifier>
where THub : Hub
{
    public IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> SetGroupResolver<TRouter>()
    where TRouter : class, IRouteResolver<TIdentifier, TEntity, TRequest>;
}

public interface IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>
where TEntity : class, IEntity<TIdentifier>
where THub : Hub
{
    public SignalRServerFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> SetAccessValidationService<THubService>()
    where THubService : class, ISignalRAccessValidator<TRequest>;
}
