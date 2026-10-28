using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Decorators;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External.Services;

public interface ICacheRegisterer<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
{
    public IEnumerable<ServiceDescriptor> GetCacheDescriptors();

    public void DecorateIdentifierReader(List<ServiceDescriptor> descriptors);
    public void DecorateAllEntityReader(List<ServiceDescriptor> descriptors);

    public void DecorateRemover(List<ServiceDescriptor> descriptors);

    public void DecorateCreator<TCreatePayload>(List<ServiceDescriptor> descriptors);
    public void DecorateUpdater<TUpdatePayload>(List<ServiceDescriptor> descriptors);
    public void DecorateGetOrCreate<TCreatePayload>(List<ServiceDescriptor> descriptors);
}

public class NoCacheRegisterer<TIdentifier, TEntity> : ICacheRegisterer<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
{
    public IEnumerable<ServiceDescriptor> GetCacheDescriptors() => [];
    public void DecorateIdentifierReader(List<ServiceDescriptor> descriptors) { }
    public void DecorateAllEntityReader(List<ServiceDescriptor> descriptors) { }
    public void DecorateRemover(List<ServiceDescriptor> descriptors) { }
    public void DecorateCreator<TCreatePayload>(List<ServiceDescriptor> descriptors) { }
    public void DecorateUpdater<TUpdatePayload>(List<ServiceDescriptor> descriptors) { }
    public void DecorateGetOrCreate<TCreatePayload>(List<ServiceDescriptor> descriptors) { }
}

public class ReadCacheAndEditingInvalidationRegisterer<TIdentifier, TEntity> : ICacheRegisterer<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>
where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
{
    public IEnumerable<ServiceDescriptor> GetCacheDescriptors() => [..PayloadHelper.Singleton<IEntityCache<TIdentifier, TEntity>, EntityCache<TIdentifier, TEntity>>()];

    public void DecorateIdentifierReader(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators.Decorate<IIdentifierReader<TIdentifier, TEntity>, CachingIdentifierReader<TIdentifier, TEntity>>(descriptors);

    public void DecorateAllEntityReader(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators.Decorate<IAllEntityReader<TIdentifier, TEntity>, CachingAllEntityReader<TIdentifier, TEntity>>(descriptors);

    public void DecorateRemover(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators.Decorate<IEntityRemover<TIdentifier, TEntity>, CacheInvalidatingEntityRemoverDecorator<TIdentifier, TEntity>>(descriptors);

    public void DecorateCreator<TCreatePayload>(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators.Decorate<IEntityCreator<TEntity, TCreatePayload>, CacheInvalidatingEntityCreatorDecorator<TIdentifier, TEntity, TCreatePayload>>(
            descriptors
        );

    public void DecorateUpdater<TUpdatePayload>(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators
            .Decorate<IEntityPayloadUpdater<TIdentifier, TEntity, TUpdatePayload>, CacheInvalidatingUpdaterDecorator<TIdentifier, TEntity, TUpdatePayload>>(descriptors);

    public void DecorateGetOrCreate<TCreatePayload>(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators
            .Decorate<IEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>, CacheInvalidatingGetOrCreateDecorator<TIdentifier, TEntity, TCreatePayload>>(
                descriptors
            );
}

public class EditingUpsertRegisterer<TIdentifier, TEntity> : ICacheRegisterer<TIdentifier, TEntity>
where TEntity : class, IEntity<TIdentifier>, IEntityWithVersion<TEntity>
where TIdentifier : struct, IEquatable<TIdentifier>, IRouteParams
{
    public IEnumerable<ServiceDescriptor> GetCacheDescriptors() => [..PayloadHelper.Singleton<IEntityCache<TIdentifier, TEntity>, VersionedEntityCache<TIdentifier, TEntity>>()];

    public void DecorateIdentifierReader(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators.Decorate<IIdentifierReader<TIdentifier, TEntity>, CachingIdentifierReader<TIdentifier, TEntity>>(descriptors);

    public void DecorateAllEntityReader(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators.Decorate<IAllEntityReader<TIdentifier, TEntity>, CachingAllEntityReader<TIdentifier, TEntity>>(descriptors);

    public void DecorateRemover(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators.Decorate<IEntityRemover<TIdentifier, TEntity>, CacheUpdatingEntityRemoverDecorator<TIdentifier, TEntity>>(descriptors);

    public void DecorateCreator<TCreatePayload>(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators.Decorate<IEntityCreator<TEntity, TCreatePayload>, CacheUpdatingEntityCreatorDecorator<TIdentifier, TEntity, TCreatePayload>>(
            descriptors
        );

    public void DecorateUpdater<TUpdatePayload>(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators
            .Decorate<IEntityPayloadUpdater<TIdentifier, TEntity, TUpdatePayload>, CacheUpdatingEntityUpdaterDecorator<TIdentifier, TEntity, TUpdatePayload>>(descriptors);

    public void DecorateGetOrCreate<TCreatePayload>(List<ServiceDescriptor> descriptors) =>
        ServiceDescriptorDecorators
            .Decorate<IEntityEnsureExistsCreator<TIdentifier, TEntity, TCreatePayload>, CacheUpdatingEntityGetOrCreateDecorator<TIdentifier, TEntity, TCreatePayload>>(
                descriptors
            );
}
