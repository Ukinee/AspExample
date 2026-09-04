using Microsoft.Extensions.DependencyInjection.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Common;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;
using Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.External;

public class RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> : IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    private DddBuilder<TIdentifier, TEntity> _builder;

    public RemoteDddBuilder(DddBuilder<TIdentifier, TEntity> builder)
    {
        _builder = builder;
    }

    internal DddFeature<TIdentifier, TEntity> Feature => _builder.Feature;

    RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>.WithMapper<TImplementation>()
    {
        _builder.Feature.Extensions.Add(collection => collection.TryAddSingleton<IMapService<TResponse, TEntity>, TImplementation>());

        return this;
    }

    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> SetCreateUseCase<TCreatePayload>()
    {
        Type[] extensions = [typeof(ExternalGatewayCreator<TTag, TParams, TIdentifier, TCreatePayload, TEntity, TResponse>)];
        
        _builder.SetEntityCreateFeature<ExternalGatewayPayloadCreateAdapter<TIdentifier, TCreatePayload, TEntity, TResponse>, TCreatePayload, CreateEntityUseCase<TCreatePayload, TEntity>>(extensions);

        return this;
    }

    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> AddUpdateUseCase<TUpdatePayload>()
    {
        Type[] extensions = [typeof(ExternalGatewayUpdater<TTag, TParams, TIdentifier, TUpdatePayload, TEntity, TResponse>)];

        _builder.AddPayloadEntityUpdateFeature<ExternalGatewayPayloadUpdaterAdapter<TIdentifier, TUpdatePayload, TEntity, TResponse>, TUpdatePayload, UpdateEntityUseCase<TIdentifier, TUpdatePayload, TEntity>>(extensions);

        return this;
    }
    
    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> WithGetOrCreate<TCreatePayload>()
    {
        _builder.AddGetOrCreateFeature<
            ExternalGatewayPayloadCreateAdapter<TIdentifier, TCreatePayload, TEntity, TResponse>,
            TCreatePayload,
            GetOrCreateEntityUseCase<TIdentifier, TCreatePayload, TEntity>
        >([]);

        return this;
    }
}

public interface IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> WithMapper<TImplementation>()
    where TImplementation : class, IMapService<TResponse, TEntity>;
}
