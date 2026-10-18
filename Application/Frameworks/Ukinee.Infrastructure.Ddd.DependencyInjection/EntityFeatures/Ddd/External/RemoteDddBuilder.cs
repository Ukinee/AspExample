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

public class RemoteDddBuilder<TTag, TIdentifier, TEntity, TResponse> :
    ICacheRegisterer<TTag, TIdentifier, TEntity, TResponse>,
    IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
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

    RemoteDddBuilder<TTag, TIdentifier, TEntity, TResponse> IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse>.WithMapper<TImplementation>()
    {
        _builder.Feature.Extensions.Add(collection => collection.TryAddSingleton<IMapService<TResponse, TEntity>, TImplementation>());

        return this;
    }

    public IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse> WithCache(CachingVariant cachingVariant)
    {
        if (!_isCacheable)
            throw new InvalidOperationException(
                $"{nameof(RemoteDddBuilder<,,,>)} is created as not cacheable. "
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
        ServiceDescriptorDecorators.Decorate<IAllEntityReader<TIdentifier, TEntity>, CachingAllEntityReader<TIdentifier, TEntity>>(Feature.EntityReader);
        ServiceDescriptorDecorators.Decorate<IEntityRemover<TIdentifier, TEntity>, CacheInvalidatingEntityRemoverDecorator<TIdentifier, TEntity>>(Feature.EntityRemover);

        return this;
    }

    public IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse> WithNoCache()
    {
        _cachingVariant = CachingVariant.DoNothing;

        return this;
    }

    public RemoteDddBuilder<TTag, TIdentifier, TEntity, TResponse> SetCreateUseCase<TCreatePayload>()
    {
        IEnumerable<ServiceDescriptor> extensions = [
            ..PayloadHelper.Service<
                IExternalGatewayCreator<TIdentifier, TCreatePayload, TResponse>,
                ExternalGatewayCreator<TTag, TIdentifier, TCreatePayload, TEntity, TResponse> // тут колдунство регистрации
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

    public RemoteDddBuilder<TTag, TIdentifier, TEntity, TResponse> AddUpdateUseCase<TUpdatePayload>()
    {
        IEnumerable<ServiceDescriptor> extensions = [
            ..PayloadHelper.Service<
                IExternalGatewayUpdater<TIdentifier, TUpdatePayload, TResponse>,
                ExternalGatewayUpdater<TTag, TIdentifier, TUpdatePayload, TEntity, TResponse>>(ServiceLifetime.Singleton),
        ];

        _builder
            .AddPayloadEntityUpdateFeature<ExternalGatewayEntityPayloadUpdaterAdapter<TIdentifier, TUpdatePayload, TEntity, TResponse>, TUpdatePayload,
                UpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload>>(ServiceLifetime.Singleton, extensions, true);

        if (_cachingVariant == CachingVariant.UpdateInvalidates)
        {
            ServiceDescriptorDecorators
                .Decorate<IEntityPayloadUpdater<TIdentifier, TEntity, TUpdatePayload>, CacheInvalidatingUpdaterDecorator<TIdentifier, TEntity, TUpdatePayload>>(
                    _builder.Feature.PayloadEntityUpdaters
                );
        }

        return this;
    }

    public RemoteDddBuilder<TTag, TIdentifier, TEntity, TResponse> WithGetOrCreate<TCreatePayload>()
    {
        _builder.AddGetOrCreateFeature<
            ExternalGatewayPayloadCreateAdapter<TIdentifier, TCreatePayload, TEntity, TResponse>,
            TCreatePayload,
            GetOrCreateEntityUseCase<TIdentifier, TEntity, TCreatePayload>
        >(ServiceLifetime.Singleton, [], true);

        return this;
    }
}

public interface ICacheRegisterer<TTag, TIdentifier, TEntity, TResponse>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
{
    public IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse> WithCache(CachingVariant cachingVariant);
    public IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse> WithNoCache();
}

public interface IMapServiceRegisterer<TTag, TIdentifier, TEntity, TResponse>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
{
    public RemoteDddBuilder<TTag, TIdentifier, TEntity, TResponse> WithMapper<TImplementation>()
    where TImplementation : class, IMapService<TResponse, TEntity>;
}
