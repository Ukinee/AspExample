using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Domain;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;
using Ukinee.Infrastructure.SignalR.Client.Contracts;
using Ukinee.Infrastructure.SignalR.Client.Services;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRClient;

public class SignalRClientFeatureBuilder<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel>
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>
{
    internal readonly SignalRClientDefinition<TSubscribeRequest, TViewModel> Feature;

    public SignalRClientFeatureBuilder(SignalRClientDefinition<TSubscribeRequest, TViewModel> feature)
    {
        Feature = feature;

        IEnumerable<ServiceDescriptor> registererDescriptors = [
            ..PayloadHelper.Singleton<IHubConnectionRegisterer<THubTag>, HubConnectionRegisterer<THubTag, TIdentifier, TEntity, TViewModel>>(),
        ];

        feature.HubConnectionRegisterer.AddRange(registererDescriptors);

        IEnumerable<ServiceDescriptor> cacheDescriptors = [
            .. PayloadHelper.Singleton<IRequestHandler<UpsertCachedEntitiesCommand<TIdentifier, TEntity>>, VoidingLocalCacheRequestHandler<TIdentifier, TEntity>>(),
            .. PayloadHelper.Singleton<IRequestHandler<InvalidateCacheCommand<TIdentifier, TEntity>>, VoidingLocalCacheRequestHandler<TIdentifier, TEntity>>(),
            .. PayloadHelper.Singleton<IRequestHandler<InvalidateCacheCommand<TIdentifier, TEntity>>, VoidingLocalCacheRequestHandler<TIdentifier, TEntity>>(),
        ];

        feature.CacheRequestHandlers.AddRange(cacheDescriptors);
    }

    public SignalRClientFeatureBuilder<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel> AddLocalCacheUpdate()
    {
        Feature.CacheRequestHandlers.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            .. PayloadHelper.Singleton<IRequestHandler<UpsertCachedEntitiesCommand<TIdentifier, TEntity>>, LocalCacheRequestHandler<TIdentifier, TEntity>>(),
            .. PayloadHelper.Singleton<IRequestHandler<InvalidateCacheCommand<TIdentifier, TEntity>>, LocalCacheRequestHandler<TIdentifier, TEntity>>(),
            .. PayloadHelper.Singleton<IRequestHandler<InvalidateCacheCommand<TIdentifier, TEntity>>, LocalCacheRequestHandler<TIdentifier, TEntity>>(),
        ];

        Feature.CacheRequestHandlers.AddRange(descriptors);

        return this;
    }
}
