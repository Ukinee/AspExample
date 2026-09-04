using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.SignalR.Contracts;
using Ukinee.Infrastructure.SignalR.Services;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.SignalR;

public class SignalRFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> :
    IGroupRoutingResolverRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>,
    IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>
where TEntity : class, IEntity<TIdentifier>
where THub : Hub
{
    internal readonly SignalRDefinition<THub, TRequest, TViewModel> Feature;

    public SignalRFeatureBuilder(SignalRDefinition<THub, TRequest, TViewModel> feature)
    {
        Feature = feature;

        Feature.AddServices(sc => sc.AddSingleton<IHubService<TRequest>, HubService<TEntity, TRequest>>());
        Feature.AddServices(sc => sc.AddSingleton<INotificationHandler<DomainEvent<TIdentifier, TEntity>>, SignalRDomainEventReceiver<TIdentifier, TEntity>>());
    }

    IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> IGroupRoutingResolverRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>.WithGroupResolver<TImplementation>()
    {
        Feature.RouteResolver = new SignalRDefinition<THub, TRequest, TViewModel>.RouteResolverDefinition {
            Type = typeof(TImplementation),
        };

        return this;
    }

    SignalRFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>.WithAccessValidationService<TImplementation>()
    {
        Feature.AccessValidator = new SignalRDefinition<THub, TRequest, TViewModel>.AccessValidatorDefinition {
            Type = typeof(TImplementation),
        };

        return this;
    }

    internal void SetSignalRSink<TSink>()
    where TSink : class, ISignalRSink<TEntity>
    {
        Feature.Sink = new SignalRDefinition<THub, TRequest, TViewModel>.SinkDefinition {
            Type = typeof(TSink),
        };
    }
}

public interface IGroupRoutingResolverRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>
where TEntity : class, IEntity<TIdentifier>
where THub : Hub
{
    public IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> WithGroupResolver<TRouter>()
    where TRouter : class, IRouteResolver<TEntity, TRequest>;
}

public interface IAccessValidationServiceRegisterer<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>
where TEntity : class, IEntity<TIdentifier>
where THub : Hub
{
    public SignalRFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> WithAccessValidationService<THubService>()
    where THubService : class, ISignalRAccessValidator<TRequest>;
}
