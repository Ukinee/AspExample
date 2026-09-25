using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Common;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;
using Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;

public class RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> : IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    private DddBuilder<TIdentifier, TEntity> _builder;
    private readonly bool _isCacheable;

    public RemoteDddBuilder(DddBuilder<TIdentifier, TEntity> builder, bool isCacheable)
    {
        _builder = builder;
        _isCacheable = isCacheable;
    }

    internal DddFeature<TIdentifier, TEntity> Feature => _builder.Feature;

    RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>.WithMapper<TImplementation>()
    {
        _builder.Feature.Extensions.Add(collection => collection.TryAddSingleton<IMapService<TResponse, TEntity>, TImplementation>());

        return this;
    }

    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> SetCreateUseCase<TCreatePayload>()
    {
        IEnumerable<ServiceDescriptor> extensions = [
            ..PayloadHelper.Singleton<
                IExternalGatewayCreator<TIdentifier, TCreatePayload, TResponse>,
                ExternalGatewayCreator<TTag, TParams, TIdentifier, TCreatePayload, TEntity, TResponse>
            >()
        ];

        _builder
            .SetEntityCreateFeature<ExternalGatewayPayloadCreateAdapter<TIdentifier, TCreatePayload, TEntity, TResponse>, TCreatePayload,
                CreateEntityUseCase<TCreatePayload, TEntity>>(extensions);

        return this;
    }

    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> AddUpdateUseCase<TUpdatePayload>()
    {
        IEnumerable<ServiceDescriptor> extensions = [
            ..PayloadHelper.Singleton<
                IExternalGatewayUpdater<TIdentifier, TUpdatePayload, TResponse>,
                ExternalGatewayUpdater<TTag, TParams, TIdentifier, TUpdatePayload, TEntity, TResponse>>()
        ];

        _builder
            .AddPayloadEntityUpdateFeature<ExternalGatewayPayloadUpdaterAdapter<TIdentifier, TUpdatePayload, TEntity, TResponse>, TUpdatePayload,
                UpdateEntityUseCase<TIdentifier, TUpdatePayload, TEntity>>(extensions);

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

    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> AddLocalCache()
    {
        if (!_isCacheable)
            throw new InvalidOperationException(
                $"{nameof(RemoteDddBuilder<,,,,>)} is created as not cacheable. "
                + $"This is probably because of startup sync. "
                + $"Please, do NOT call {nameof(AddLocalCache)} inside of {nameof(RemoteDddBuilderProxy<,,>.RegisterStartupRemoteSynchronizationToMemoryRepository)}"
            );

        List<ServiceDescriptor> descriptors = [
            ServiceDescriptor.Singleton<IEntityCache<TIdentifier, TEntity>>(sc => sc.GetRequiredService<EntityCache<TIdentifier, TEntity>>()),
            ServiceDescriptor.Singleton<EntityCache<TIdentifier, TEntity>, EntityCache<TIdentifier, TEntity>>(),
        ];

        Feature.IdentifierReader.AddRange(descriptors);

        ServiceDescriptorDecorators.Decorate<IIdentifierReader<TIdentifier, TEntity>>(
            Feature.IdentifierReader,
            innerType => typeof(CachingIdentifierReader<,,>).MakeGenericType(typeof(TIdentifier), typeof(TEntity), innerType)
        );

        ServiceDescriptorDecorators.Decorate<IEntityReader<TIdentifier, TEntity>>(
            Feature.EntityReader,
            innerType => typeof(CachingEntityReader<,,>).MakeGenericType(typeof(TIdentifier), typeof(TEntity), innerType)
        );

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
