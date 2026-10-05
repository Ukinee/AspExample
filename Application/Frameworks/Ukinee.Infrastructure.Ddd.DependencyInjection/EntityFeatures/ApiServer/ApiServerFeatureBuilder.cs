using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Users.Common.ValueObjects;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;

public class ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> :
    ICrudGroupConfigurator<TIdentifier, TEntity, TParams, TResponse>
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>
where TParams : struct, IRouteParams<TParams, TIdentifier>
{
    private readonly Func<TParams, UserContext, TIdentifier> _idFactory;
    private readonly AuthorizationPolicyDefinition<TIdentifier, TEntity> _policyDefinition;
    internal readonly ApiServerDefinition Feature;

    public ApiServerFeatureBuilder(
        Func<TParams, UserContext, TIdentifier> idFactory,
        AuthorizationPolicyDefinition<TIdentifier, TEntity> policyDefinition,
        ApiServerDefinition feature
    )
    {
        _idFactory = idFactory;
        _policyDefinition = policyDefinition;
        Feature = feature;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> RegisterRead(Action<RouteHandlerBuilder>? builder = null)
    {
        return WithFindMany(builder)
            .WithGet(builder)
            .WithGetAll(builder);
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> RegisterReadDelete(Action<RouteHandlerBuilder>? builder = null)
    {
        return RegisterRead(builder)
            .WithDelete(builder);
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> RegisterCrud<TCreatePayload, TUpdatePayload>()
    {
        return WithFindMany()
                .WithGetAll()
                .WithGet()
                .WithDelete()
                .WithDeleteRange()
                .WithThrowingOnDuplicatesCreateMany<TCreatePayload>()
                .WithThrowingOnDuplicatesCreate<TCreatePayload>()
                .AddUpdate<TUpdatePayload>()
                .AddUpdateMany<TUpdatePayload>()
            ;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> RegisterCrud<TPayload>()
    {
        return WithFindMany()
                .WithGetAll()
                .WithGet()
                .WithDelete()
                .WithDeleteRange()
                .WithThrowingOnDuplicatesCreateMany<TPayload>()
                .WithThrowingOnDuplicatesCreate<TPayload>()
                .AddUpdate<TPayload>()
                .AddUpdateMany<TPayload>()
            ;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> RegisterUpdate<TPayload>()
    {
        return AddUpdate<TPayload>()
            .AddUpdateMany<TPayload>();
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> RegisterCrd<TCreatePayload>()
    {
        return WithFindMany()
            .WithGetAll()
            .WithGet()
            .WithDelete()
            .WithDeleteRange()
            .WithThrowingOnDuplicatesCreateMany<TCreatePayload>()
            .WithThrowingOnDuplicatesCreate<TCreatePayload>();
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> RegisterCreate<TCreatePayload>()
    {
        return WithThrowingOnDuplicatesCreateMany<TCreatePayload>()
            .WithThrowingOnDuplicatesCreate<TCreatePayload>();
    }

    ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse>
        ICrudGroupConfigurator<TIdentifier, TEntity, TParams, TResponse>.WithGroupConfiguration(Action<RouteGroupBuilder> builder)
    {
        Feature.ConfigureGroup(builder);

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithGetAll(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapGet(
                        RelationalPathUtils.FindAll<TEntity>(),
                        (
                            [FromServices] IGetAllEntityUseCase<TIdentifier, TEntity> useCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var found = useCase.ExecuteAll(userContext, ct);

                            return TypedResults.Ok(found.Select(mapper.Map<TResponse>));
                        }
                    )
                    .WithName($"GetAll{typeof(TEntity).Name}");

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Read);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithFindMany(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.FindMany<TEntity>(),
                        (
                            [FromBody] SearchByIdRequest<TIdentifier> request,
                            [FromServices] IGetEntityUseCase<TIdentifier, TEntity> useCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var result = useCase.ExecuteSoft(userContext, request.Identifiers, ct).Select(mapper.Map<TResponse>);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName($"FindMany{typeof(TEntity).Name}");

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Read);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithGet(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                RouteHandlerBuilder endpoint = group
                    .MapGet(
                        RelationalPathUtils.Find<TEntity>(TParams.RouteTemplate),
                        async (
                            [AsParameters] TParams routeParams,
                            [FromServices] IGetEntityUseCase<TIdentifier, TEntity> useCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();
                            var id = _idFactory(routeParams, userContext);
                            var entity = await useCase.Execute(userContext, id, ct);
                            var result = mapper.Map<TResponse>(entity);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName($"GetSpecific{typeof(TEntity).Name}");

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Read);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> AddUpdate<TUpdatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapPut(
                        RelationalPathUtils.Update<TEntity, TUpdatePayload>(TParams.RouteTemplate),
                        async (
                            [AsParameters] TParams routeParams,
                            [FromBody] TUpdatePayload request,
                            [FromServices] IUpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload> updateUseCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();
                            var id = _idFactory(routeParams, userContext);

                            var updatedEntity = await updateUseCase.Execute(userContext, id, request, ct);
                            var result = mapper.Map<TResponse>(updatedEntity);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName(EndpointsNames.Update<TEntity, TUpdatePayload>());

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Update);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> AddUpdateMany<TUpdatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapPut(
                        RelationalPathUtils.Update<TEntity, TUpdatePayload>(TParams.RouteTemplate),
                        async (
                            [FromBody] IReadOnlyCollection<UpdateEntityRequest<TParams, TUpdatePayload>> paramsRequest,
                            [FromServices] IUpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload> updateUseCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var requests = paramsRequest
                                .Select(request => new UpdateEntityRequest<TIdentifier, TUpdatePayload> {
                                        Identifier = _idFactory(request.Identifier, userContext),
                                        Payload = request.Payload,
                                    }
                                )
                                .ToList();

                            var updatedEntity = await updateUseCase.Execute(userContext, requests, ct);
                            var result = mapper.Map<TResponse>(updatedEntity);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName(EndpointsNames.UpdateMany<TEntity, TUpdatePayload>());

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Update);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithThrowingOnDuplicatesCreate<TCreatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.Create<TEntity>(),
                        async (
                            [FromBody] TCreatePayload request,
                            [FromServices] ICreateEntityUseCase<TEntity, TCreatePayload> useCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var entity = await useCase.Execute(userContext, request, ct);
                            var result = mapper.Map<TResponse>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"Create{typeof(TEntity).Name}");

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Create);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithThrowingOnDuplicatesCreateMany<TCreatePayload>(
        Action<RouteHandlerBuilder>? builder = null
    )
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.CreateMany<TEntity>(),
                        async (
                            [FromBody] IReadOnlyCollection<TCreatePayload> requests,
                            [FromServices] ICreateEntityUseCase<TEntity, TCreatePayload> useCase,
                            [FromServices] ILogger<ICreateEntityUseCase<TEntity, TCreatePayload>> logger,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            logger.LogInformation("Creating {TEntityName} with user context {UserContext}", typeof(TEntity).Name, userContext);
                            
                            var entity = await useCase.Execute(userContext, requests, ct);
                            var result = mapper.Map<List<TResponse>>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"CreateMany{typeof(TEntity).Name}");

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Create);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithEnsureExistsCreate<TCreatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.EnsureExists<TEntity>(TParams.RouteTemplate),
                        async (
                            [AsParameters] TParams routeParams,
                            [FromBody] TCreatePayload request,
                            [FromServices] IGetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload> useCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var id = _idFactory(routeParams, userContext);

                            var entity = await useCase.Execute(userContext, id, request, ct);
                            var result = mapper.Map<TResponse>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"EnsureExists{typeof(TEntity).Name}");

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Create);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithEnsureExistsCreateMany<TCreatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.EnsureExistsMany<TEntity>(),
                        async (
                            [FromBody] IReadOnlyCollection<GetOrCreateRequest<TParams, TCreatePayload>> paramsRequests,
                            [FromServices] IGetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload> useCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var requests = paramsRequests
                                .Select(request => new GetOrCreateRequest<TIdentifier, TCreatePayload> {
                                        Identifier = _idFactory(request.Identifier, userContext),
                                        Payload = request.Payload,
                                    }
                                )
                                .ToList();

                            var entity = await useCase.Execute(userContext, requests, ct);
                            var result = mapper.Map<List<TResponse>>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"EnsureExistsMany{typeof(TEntity).Name}");

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Create);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithDelete(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapDelete(
                        RelationalPathUtils.Delete<TEntity>(TParams.RouteTemplate),
                        async (
                            [AsParameters] TParams routeParams,
                            [FromServices] IRemoveEntityUseCase<TIdentifier, TEntity> removeUseCase,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();
                            var id = _idFactory(routeParams, userContext);

                            await removeUseCase.Execute(userContext, id, ct);

                            return TypedResults.NoContent();
                        }
                    )
                    .WithName($"Delete{typeof(TEntity).Name}");

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Delete);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithDeleteRange(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapDelete(
                        RelationalPathUtils.DeleteMany<TEntity>(),
                        async (
                            [FromBody] List<TParams> identifiers,
                            [FromServices] IRemoveEntityUseCase<TIdentifier, TEntity> removeUseCase,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();
                            var id = identifiers.Select(param => _idFactory(param, userContext)).ToList();

                            await removeUseCase.Execute(userContext, id, ct);

                            return TypedResults.NoContent();
                        }
                    )
                    .WithName($"DeleteRange{typeof(TEntity).Name}");

                ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Delete);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> AddRequestHandler<TPayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapDelete(
                        RelationalPathUtils.EffectMany<TEntity, TPayload>(),
                        async (
                            [FromBody] IReadOnlyCollection<TPayload> payloads,
                            [FromServices] IMediator mediator,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var identifierRequest = new PayloadRequest<TPayload> {
                                UserContext = userContext,
                                Contents = payloads,
                            };

                            await mediator.Send(identifierRequest, ct);

                            return TypedResults.Ok();
                        }
                    )
                    .WithName($"Execute{typeof(TPayload).Name}In{typeof(TEntity).Name}Context");

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> AddRequestHandler<TPayload, THandlerResponse, TEndpointResponse>(
        Action<RouteHandlerBuilder>? builder = null
    )
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapDelete(
                        RelationalPathUtils.EffectMany<TEntity, TPayload>(),
                        async (
                            [FromBody] IReadOnlyCollection<TPayload> payloads,
                            [FromServices] IMapper mapper,
                            [FromServices] IMediator mediator,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var identifierRequest = new PayloadRequest<TPayload, THandlerResponse> {
                                UserContext = userContext,
                                Contents = payloads,
                            };

                            var result = await mediator.Send(identifierRequest, ct);

                            var response = mapper.Map<List<TEndpointResponse>>(result);

                            return TypedResults.Ok(response);
                        }
                    )
                    .WithName($"Execute{typeof(TPayload).Name}In{typeof(TEntity).Name}ContextAndGet{typeof(TEndpointResponse).Name}");

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }
}

public interface ICrudGroupConfigurator<TIdentifier, TEntity, TParams, TResponse>
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>
where TParams : struct, IRouteParams<TParams, TIdentifier>
{
    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithGroupConfiguration(Action<RouteGroupBuilder> builder);
}
