using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRClient;

public static class SignalRClientFeatureBuilderExtensions
{
    extension<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel>(
        SignalRClientFeatureBuilder<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel> builder
    )
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public SignalRClientFeatureBuilder<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel> SetInvalidatingCacheHandlers()
        {
            return builder.SetCacheHandlers<InvalidatingCacheRequestHandler<TIdentifier, TEntity>>();
        }
    }
    
    extension<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel>(
        SignalRClientFeatureBuilder<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel> builder
    )
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>, IEntityWithVersion<TEntity>
    {
        public SignalRClientFeatureBuilder<TTag, THubTag, TIdentifier, TEntity, TSubscribeRequest, TViewModel> SetVersionAwareCacheHandlers()
        {
            return builder.SetCacheHandlers<UpsertingCacheRequestHandler<TIdentifier, TEntity>>();
        }
    }
}
