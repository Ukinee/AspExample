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

public class ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse>
where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
where TEntity : class, IEntity<TIdentifier>
{
    private readonly AuthorizationPolicyDefinition<TIdentifier, TEntity>? _policyDefinition;
    internal readonly ApiServerDefinition Feature;

    public ApiServerFeatureBuilder(
        AuthorizationPolicyDefinition<TIdentifier, TEntity>? policyDefinition,
        ApiServerDefinition feature
    )
    {
        _policyDefinition = policyDefinition;
        Feature = feature;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> RegisterRead(Action<RouteHandlerBuilder>? builder = null)
    {
        return WithFindMany(builder)
            .WithGet(builder)
            .WithGetAll(builder);
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> RegisterDelete(Action<RouteHandlerBuilder>? builder = null)
    {
        return WithDelete(builder)
            .WithDeleteRange(builder);
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> RegisterCrud<TCreatePayload, TUpdatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        return RegisterRead(builder)
                .RegisterDelete(builder)
                .RegisterCreate<TCreatePayload>(builder)
                .RegisterUpdate<TUpdatePayload>(builder)
            ;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> RegisterCrud<TPayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        return RegisterRead(builder)
                .RegisterDelete(builder)
                .RegisterCreate<TPayload>(builder)
                .RegisterUpdate<TPayload>(builder)
            ;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> RegisterUpdate<TPayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        return AddUpdate<TPayload>(builder)
            .AddUpdateMany<TPayload>(builder);
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> RegisterCreate<TCreatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        return WithThrowingOnDuplicatesCreateMany<TCreatePayload>(builder)
            .WithThrowingOnDuplicatesCreate<TCreatePayload>(builder);
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> AddGroupConfiguration(Action<RouteGroupBuilder> builder)
    {
        Feature.ConfigureGroup(builder);

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithGetAll(Action<RouteHandlerBuilder>? builder = null)
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

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Read);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithFindMany(Action<RouteHandlerBuilder>? builder = null)
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

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Read);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithGet(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                RouteHandlerBuilder endpoint = group
                    .MapGet(
                        RelationalPathUtils.Find<TEntity>(TIdentifier.RouteTemplate),
                        async (
                            [AsParameters] TIdentifier identifier,
                            [FromServices] IGetEntityUseCase<TIdentifier, TEntity> useCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();
                            var entity = await useCase.Execute(userContext, identifier, ct);
                            var result = mapper.Map<TResponse>(entity);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName($"GetSpecific{typeof(TEntity).Name}");

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Read);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> AddUpdate<TUpdatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapPut(
                        RelationalPathUtils.Update<TEntity, TUpdatePayload>(TIdentifier.RouteTemplate),
                        async (
                            [AsParameters] TIdentifier identifier,
                            [FromBody] TUpdatePayload request,
                            [FromServices] IUpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload> updateUseCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var updatedEntity = await updateUseCase.Execute(userContext, identifier, request, ct);
                            var result = mapper.Map<TResponse>(updatedEntity);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName(EndpointsNames.Update<TEntity, TUpdatePayload>());

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Update);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> AddUpdateMany<TUpdatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapPut(
                        RelationalPathUtils.UpdateMany<TEntity, TUpdatePayload>(),
                        async (
                            [FromBody] IReadOnlyCollection<UpdateEntityRequest<TIdentifier, TUpdatePayload>> requests,
                            [FromServices] IUpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload> updateUseCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var updatedEntity = await updateUseCase.Execute(userContext, requests, ct);
                            var result = mapper.Map<TResponse>(updatedEntity);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName(EndpointsNames.UpdateMany<TEntity, TUpdatePayload>());

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Update);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithThrowingOnDuplicatesCreate<TCreatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.Create<TEntity, TCreatePayload>(),
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
                    .WithName($"Create{typeof(TEntity).Name}With{typeof(TCreatePayload).Name}");

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Create);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithThrowingOnDuplicatesCreateMany<TCreatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.CreateMany<TEntity, TCreatePayload>(),
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
                    .WithName($"CreateMany{typeof(TEntity).Name}With{typeof(TCreatePayload).Name}");

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Create);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithEnsureExistsCreate<TCreatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.EnsureExists<TEntity>(TIdentifier.RouteTemplate),
                        async (
                            [AsParameters] TIdentifier identifier,
                            [FromBody] TCreatePayload request,
                            [FromServices] IGetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload> useCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var entity = await useCase.Execute(userContext, identifier, request, ct);
                            var result = mapper.Map<TResponse>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"EnsureExists{typeof(TEntity).Name}");

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Create);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithEnsureExistsCreateMany<TCreatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.EnsureExistsMany<TEntity>(),
                        async (
                            [FromBody] IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TCreatePayload>> requests,
                            [FromServices] IGetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload> useCase,
                            [FromServices] IMapper mapper,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            var entity = await useCase.Execute(userContext, requests, ct);
                            var result = mapper.Map<List<TResponse>>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"EnsureExistsMany{typeof(TEntity).Name}");

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Create);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithDelete(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapDelete(
                        RelationalPathUtils.Delete<TEntity>(TIdentifier.RouteTemplate),
                        async (
                            [AsParameters] TIdentifier identifier,
                            [FromServices] IRemoveEntityUseCase<TIdentifier, TEntity> removeUseCase,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            await removeUseCase.Execute(userContext, identifier, ct);

                            return TypedResults.NoContent();
                        }
                    )
                    .WithName($"Delete{typeof(TEntity).Name}");

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Delete);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> WithDeleteRange(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapDelete(
                        RelationalPathUtils.DeleteMany<TEntity>(),
                        async (
                            [FromBody] List<TIdentifier> identifiers,
                            [FromServices] IRemoveEntityUseCase<TIdentifier, TEntity> removeUseCase,
                            [FromServices] IUserContextProvider userContextProvider,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = userContextProvider.GetActiveUserContext();

                            await removeUseCase.Execute(userContext, identifiers, ct);

                            return TypedResults.NoContent();
                        }
                    )
                    .WithName($"DeleteRange{typeof(TEntity).Name}");

                if (_policyDefinition != null)
                    ApiAuthorizationHelper.ApplyPolicy(endpoint, _policyDefinition, AuthorizedOperation.Delete);

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> AddRequestHandler<TPayload>(Action<RouteHandlerBuilder>? builder = null)
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

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TResponse> AddRequestHandler<TPayload, THandlerResponse, TEndpointResponse>(
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
