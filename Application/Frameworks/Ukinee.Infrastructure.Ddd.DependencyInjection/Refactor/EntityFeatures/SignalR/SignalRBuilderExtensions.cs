using Microsoft.AspNetCore.SignalR;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.SignalR.Services;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.SignalR;

public static class SignalRBuilderExtensions
{
    extension<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel>(SignalRFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> definition)
    where TEntity : class, IEntity<TIdentifier>
    where THub : Hub
    where TIdentifier : notnull
    {
        public SignalRFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> WithBatchedSink()
        {
            definition.SetSignalRSink<BatchedSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub>>();

            return definition;
        }

        public SignalRFeatureBuilder<THub, TTag, TIdentifier, TEntity, TRequest, TViewModel> WithRealTimeSink()
        {
            definition.SetSignalRSink<ImmediateSignalRSink<TRequest, TEntity, TViewModel, THub>>();

            return definition;
        }
    }
}
