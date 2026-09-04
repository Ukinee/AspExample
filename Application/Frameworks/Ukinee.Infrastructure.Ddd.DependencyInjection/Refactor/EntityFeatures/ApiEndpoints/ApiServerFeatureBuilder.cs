using MapsterMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.ApiEndpoints;

public class ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> :
    ICrudGroupConfigurator<TIdentifier, TEntity, TParams, TResponse>
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>
where TParams : struct, IRouteParams<TParams, TIdentifier>
{
    private readonly Func<TParams, UserContext, TIdentifier> _idFactory;
    internal readonly ApiServerDefinition Feature;

    public ApiServerFeatureBuilder(Func<TParams, UserContext, TIdentifier> idFactory, ApiServerDefinition feature)
    {
        _idFactory = idFactory;
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
            .WithCreateMany<TCreatePayload>()
            .WithThrowingOnDuplicatesCreate<TCreatePayload>()
            .WithUpdate<TUpdatePayload>();
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> RegisterCrud<TPayload>()
    {
        return WithFindMany()
            .WithGetAll()
            .WithGet()
            .WithDelete()
            .WithDeleteRange()
            .WithCreateMany<TPayload>()
            .WithThrowingOnDuplicatesCreate<TPayload>()
            .WithUpdate<TPayload>();
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> RegisterCrd<TCreatePayload>()
    {
        return WithFindMany()
            .WithGetAll()
            .WithGet()
            .WithDelete()
            .WithDeleteRange()
            .WithCreateMany<TCreatePayload>()
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
                        ([FromServices] IGetEntityUseCase<TIdentifier, TEntity> useCase, [FromServices] IMapper mapper, CancellationToken ct) =>
                        {
                            var userContext = UserContext.Test;

                            var found = useCase.ExecuteAll(userContext, ct);

                            return TypedResults.Ok(found.Select(mapper.Map<TResponse>));
                        }
                    )
                    .WithName($"GetAll{typeof(TEntity).Name}");

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
                        ([FromBody] SearchByIdRequest<TIdentifier> request, [FromServices] IGetEntityUseCase<TIdentifier, TEntity> useCase, [FromServices] IMapper mapper, CancellationToken ct) =>
                        {
                            var userContext = UserContext.Test;

                            var result = useCase.ExecuteSoft(userContext, request.Identifiers, ct).Select(mapper.Map<TResponse>);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName($"FindMany{typeof(TEntity).Name}");

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
                        async ([AsParameters] TParams routeParams, [FromServices] IGetEntityUseCase<TIdentifier, TEntity> useCase, [FromServices] IMapper mapper, CancellationToken ct) =>
                        {
                            var userContext = UserContext.Test;
                            var id = _idFactory(routeParams, userContext);
                            var entity = await useCase.Execute(userContext, id, ct);
                            var result = mapper.Map<TResponse>(entity);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName($"GetSpecific{typeof(TEntity).Name}");

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithUpdate<TUpdatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, _) =>
            {
                var endpoint = group
                    .MapPut(
                        RelationalPathUtils.Update<TEntity, TUpdatePayload>(TParams.RouteTemplate),
                        async (
                            [AsParameters] TParams routeParams,
                            [FromBody] TUpdatePayload request,
                            [FromServices] IUpdateEntityUseCase<TIdentifier, TUpdatePayload, TEntity> updateUseCase,
                            [FromServices] IMapper mapper,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = UserContext.Test;
                            var id = _idFactory(routeParams, userContext);

                            var updatedEntity = await updateUseCase.Execute(userContext, id, request);
                            var result = mapper.Map<TResponse>(updatedEntity);

                            return TypedResults.Ok(result);
                        }
                    )
                    .WithName(RelationalPathUtils.UpdateEndpointName<TEntity, TUpdatePayload>());

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
                        async ([FromBody] TCreatePayload request, [FromServices] ICreateEntityUseCase<TCreatePayload, TEntity> useCase, [FromServices] IMapper mapper) =>
                        {
                            var userContext = UserContext.Test;

                            var entity = await useCase.Execute(userContext, request);
                            var result = mapper.Map<TResponse>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"Create{typeof(TEntity).Name}");

                builder?.Invoke(endpoint);
            }
        );

        return this;
    }

    public ApiServerFeatureBuilder<TIdentifier, TEntity, TParams, TResponse> WithCreateMany<TCreatePayload>(Action<RouteHandlerBuilder>? builder = null)
    {
        Feature.AddEndpoint((group, def) =>
            {
                var endpoint = group
                    .MapPost(
                        RelationalPathUtils.CreateMany<TEntity>(),
                        async ([FromBody] IReadOnlyCollection<TCreatePayload> requests, [FromServices] ICreateEntityUseCase<TCreatePayload, TEntity> useCase, [FromServices] IMapper mapper) =>
                        {
                            var userContext = UserContext.Test;

                            var entity = await useCase.Execute(userContext, requests);
                            var result = mapper.Map<List<TResponse>>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"CreateMany{typeof(TEntity).Name}");

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
                            [FromServices] IGetOrCreateEntityUseCase<TIdentifier, TCreatePayload, TEntity> useCase,
                            [FromServices] IMapper mapper,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = UserContext.Test;

                            var id = _idFactory(routeParams, userContext);

                            var entity = await useCase.Execute(userContext, id, request, ct);
                            var result = mapper.Map<TResponse>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"EnsureExists{typeof(TEntity).Name}");

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
                            [FromBody] IReadOnlyCollection<GetOrCreateRequest<TIdentifier, TCreatePayload>> requests,
                            [FromServices] IGetOrCreateEntityUseCase<TIdentifier, TCreatePayload, TEntity> useCase,
                            [FromServices] IMapper mapper,
                            CancellationToken ct
                        ) =>
                        {
                            var userContext = UserContext.Test;

                            var entity = await useCase.Execute(userContext, requests, ct);
                            var result = mapper.Map<List<TResponse>>(entity);

                            return TypedResults.Created($"{def.BaseRoute}", result);
                        }
                    )
                    .WithName($"EnsureExistsMany{typeof(TEntity).Name}");

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
                        async ([AsParameters] TParams routeParams, IGetEntityUseCase<TIdentifier, TEntity> useCase, IRemoveEntityUseCase<TEntity> removeUseCase, CancellationToken ct) =>
                        {
                            var userContext = UserContext.Test;
                            var id = _idFactory(routeParams, userContext);

                            var entity = await useCase.Execute(userContext, id, ct);
                            await removeUseCase.Execute(userContext, entity, ct);

                            return TypedResults.NoContent();
                        }
                    )
                    .WithName($"Delete{typeof(TEntity).Name}");

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
                        async ([FromBody] List<TParams> identifiers, [FromServices] IRemoveEntityUseCase<TIdentifier, TEntity> removeUseCase, CancellationToken ct) =>
                        {
                            var userContext = UserContext.Test;
                            var id = identifiers.Select(param => _idFactory(param, userContext)).ToList();

                            await removeUseCase.Execute(userContext, id, ct);

                            return TypedResults.NoContent();
                        }
                    )
                    .WithName($"DeleteRange{typeof(TEntity).Name}");

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
