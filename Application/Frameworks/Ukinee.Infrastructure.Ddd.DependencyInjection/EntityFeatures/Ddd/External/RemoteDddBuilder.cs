using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Decorators;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Common;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;
using Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;

public enum CachingVariant
{
    DoNothing,
    UpdateInvalidates,
    UpdateUpserts,
}

public class RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> :
    ICacheRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>,
    IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    private DddBuilder<TTag, TIdentifier, TEntity> _builder;
    private readonly bool _isCacheable;
    private CachingVariant _cachingVariant = CachingVariant.DoNothing;

    public RemoteDddBuilder(DddBuilder<TTag, TIdentifier, TEntity> builder, bool isCacheable)
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

    public IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse> WithCache(CachingVariant cachingVariant)
    {
        if (!_isCacheable)
            throw new InvalidOperationException(
                $"{nameof(RemoteDddBuilder<,,,,>)} is created as not cacheable. "
                + $"This is probably because of startup sync. "
                + $"Please, do NOT call {nameof(WithCache)} inside of {nameof(RemoteDddBuilderProxy<,,>.RegisterStartupRemoteSynchronizationToMemoryRepository)}"
            );

        if (cachingVariant == CachingVariant.DoNothing)
        {
            _cachingVariant = cachingVariant;

            return this;
        }

        if (cachingVariant == CachingVariant.UpdateUpserts)
        {
            throw new NotImplementedException($"{nameof(CachingVariant)}.{CachingVariant.UpdateUpserts} not implemented due to lack of entity versioning");
        }

        _cachingVariant = cachingVariant;

        List<ServiceDescriptor> descriptors = [
            ServiceDescriptor.Singleton<IEntityCache<TIdentifier, TEntity>>(sc => sc.GetRequiredService<EntityCache<TIdentifier, TEntity>>()),
            ServiceDescriptor.Singleton<EntityCache<TIdentifier, TEntity>, EntityCache<TIdentifier, TEntity>>(),
        ];

        Feature.IdentifierReader.AddRange(descriptors);

        ServiceDescriptorDecorators.Decorate<IIdentifierReader<TIdentifier, TEntity>, CachingIdentifierReader<TIdentifier, TEntity>>(Feature.IdentifierReader);
        ServiceDescriptorDecorators.Decorate<IEntityReader<TIdentifier, TEntity>, CachingEntityReader<TIdentifier, TEntity>>(Feature.EntityReader);
        ServiceDescriptorDecorators.Decorate<IEntityRemover<TIdentifier, TEntity>, CacheInvalidatingEntityRemoverDecorator<TIdentifier, TEntity>>(Feature.EntityRemover);

        return this;
    }

    public IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse> WithNoCache()
    {
        _cachingVariant = CachingVariant.DoNothing;

        return this;
    }

    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> SetCreateUseCase<TCreatePayload>()
    {
        IEnumerable<ServiceDescriptor> extensions = [
            ..PayloadHelper.Service<
                IExternalGatewayCreator<TIdentifier, TCreatePayload, TResponse>,
                ExternalGatewayCreator<TTag, TParams, TIdentifier, TCreatePayload, TEntity, TResponse>
            >(ServiceLifetime.Singleton),
        ];

        _builder
            .SetEntityCreateFeature<ExternalGatewayPayloadCreateAdapter<TIdentifier, TCreatePayload, TEntity, TResponse>, TCreatePayload,
                CreateEntityUseCase<TEntity, TCreatePayload>>(ServiceLifetime.Singleton, extensions, true);

        if (_cachingVariant == CachingVariant.UpdateInvalidates)
        {
            ServiceDescriptorDecorators.Decorate<IEntityCreator<TEntity, TCreatePayload>, CacheInvalidatingEntityCreatorDecorator<TIdentifier, TEntity, TCreatePayload>>(
                _builder.Feature.EntityCreator
            );
        }

        return this;
    }

    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> AddUpdateUseCase<TUpdatePayload>()
    {
        IEnumerable<ServiceDescriptor> extensions = [
            ..PayloadHelper.Service<
                IExternalGatewayUpdater<TIdentifier, TUpdatePayload, TResponse>,
                ExternalGatewayUpdater<TTag, TParams, TIdentifier, TUpdatePayload, TEntity, TResponse>>(ServiceLifetime.Singleton),
        ];

        _builder
            .AddPayloadEntityUpdateFeature<ExternalGatewayPayloadUpdaterAdapter<TIdentifier, TUpdatePayload, TEntity, TResponse>, TUpdatePayload,
                UpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload>>(ServiceLifetime.Singleton, extensions, true);

        if (_cachingVariant == CachingVariant.UpdateInvalidates)
        {
            ServiceDescriptorDecorators
                .Decorate<IPayloadEntityUpdater<TIdentifier, TEntity, TUpdatePayload>, CacheInvalidatingEntityUpdaterDecorator<TIdentifier, TEntity, TUpdatePayload>>(
                    _builder.Feature.PayloadEntityUpdaters
                );
        }

        return this;
    }

    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> WithGetOrCreate<TCreatePayload>()
    {
        _builder.AddGetOrCreateFeature<
            ExternalGatewayPayloadCreateAdapter<TIdentifier, TCreatePayload, TEntity, TResponse>,
            TCreatePayload,
            GetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload>
        >(ServiceLifetime.Singleton, [], true);

        return this;
    }
}

public interface ICacheRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    public IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse> WithCache(CachingVariant cachingVariant);
    public IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse> WithNoCache();
}

public interface IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>
where TParams : IRouteParams<TParams, TIdentifier>
{
    public RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse> WithMapper<TImplementation>()
    where TImplementation : class, IMapService<TResponse, TEntity>;
}
