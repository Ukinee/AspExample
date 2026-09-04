using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.ApiEndpoints;

public class ApiServerDefinition : IBuildable
{
    public required string BaseRoute { get; init; }
    public required string Tag { get; init; }

    private Action<RouteGroupBuilder>? _groupConfiguration;

    private readonly List<Action<RouteGroupBuilder, ApiServerDefinition>> _endpoints = new();

    public void ConfigureGroup(Action<RouteGroupBuilder> config)
    {
        _groupConfiguration += config;
    }

    public void AddEndpoint(Action<RouteGroupBuilder, ApiServerDefinition> endpoint)
    {
        _endpoints.Add(endpoint);
    }

    public void RegistrationAction(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute).WithTags(Tag);

        _groupConfiguration?.Invoke(group);

        foreach (var endpoint in _endpoints)
        {
            endpoint(group, this);
        }
    }

    public void Build(IReadOnlyCollection<Type> allowedInterfaces, IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(this);
    }
}
