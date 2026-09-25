using Microsoft.AspNetCore.SignalR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.SignalR.Server.Services;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRServer;

public static class SignalRServerBuilderExtensions
{
    extension<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>(SignalRServerFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> definition)
    where TEntity : class, IEntity<TIdentifier>
    where THub : Hub
    where TIdentifier : notnull
    {
        public SignalRServerFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> WithBatchedSink()
        {
            definition.SetSignalRSink<BatchedSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub>>();

            return definition;
        }

        public SignalRServerFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> WithRealTimeSink()
        {
            definition.SetSignalRSink<ImmediateSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub>>();

            return definition;
        }
    }
}
