using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Common;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.External;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;
using Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.EntityFeatures.Ddd.External;

file class ProxyHelper
{
    public static void RegisterGateway<TTag, TParams, TIdentifier, TEntity, TResponse>(IServiceCollection serviceCollection)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TParams : IRouteParams<TParams, TIdentifier>
    where TResponse : class
    {
        serviceCollection
            .AddSingleton<IExternalGateway<TIdentifier, TResponse>, ExternalGateway<TTag, TParams, TIdentifier, TEntity, TResponse>>()
            ;
    }

}

public class RemoteDddBuilderProxy<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> definition)
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>
{
    public ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> RegisterReadOnly<TParams, TResponse>(
        Func<IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>, RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse>> factory
    )
    where TParams : IRouteParams<TParams, TIdentifier>
    where TResponse : class
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.Feature.Extensions.Add(ProxyHelper.RegisterGateway<TTag, TParams, TIdentifier, TEntity, TResponse>);

        builder.WithEntityReader<ExternalGatewayAdapter<TIdentifier, TEntity, TResponse>>();
        builder.WithIdentifierReader<ExternalGatewayAdapter<TIdentifier, TEntity, TResponse>, GetEntityUseCase<TIdentifier, TEntity>>();

        var remoteBuilder = new RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse>(builder);
        factory.Invoke(remoteBuilder);

        definition.Features.Add(remoteBuilder.Feature);

        return definition;
    }
    
    public ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> RegisterStaticDataWithStartupRemoteSynchronizationToMemoryRepository<TParams, TResponse, TWeight, TDataSource>(
        Func<IMapServiceRegisterer<TTag, TParams, TIdentifier, TEntity, TResponse>, RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse>> factory
    )
    where TParams : IRouteParams<TParams, TIdentifier>
    where TWeight : ISynchronizationOrderByPriority
    where TDataSource : class, ISynchronizationDataSource<TEntity>
    where TResponse : class
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.Feature.Extensions.Add(CommonProxyHelper.RegisterStartupSynchronization<TIdentifier, TEntity, TWeight, TDataSource>);
        builder.Feature.Extensions.Add(ProxyHelper.RegisterGateway<TTag, TParams, TIdentifier, TEntity, TResponse>);

        builder.WithRepository<InMemoryDictionaryRepository<TIdentifier, TEntity>>();
        builder.WithEntityReader<TrackedReader<TIdentifier, TEntity>>();
        builder.WithIdentifierReader<TrackedReader<TIdentifier, TEntity>, GetEntityUseCase<TIdentifier, TEntity>>();
        builder.WithSpecificationReader<TrackedReader<TIdentifier, TEntity>>();

        var remoteBuilder = new RemoteDddBuilder<TTag, TParams, TIdentifier, TEntity, TResponse>(builder);
        factory.Invoke(remoteBuilder);

        definition.Features.Add(remoteBuilder.Feature);

        return definition;
    }
}

public static class ExternalEntityDefinitionExtensions
{
    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public RemoteDddBuilderProxy<TTag, TIdentifier, TEntity> Ddd => new RemoteDddBuilderProxy<TTag, TIdentifier, TEntity>(definition);
    }
}
