using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Common;
using Ukinee.Infrastructure.Ddd.External.Api.Domain;
using Ukinee.Infrastructure.Ddd.External.Api.GatewayServices;
using Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;

file class ProxyHelper
{
    public static void RegisterGateway<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>(IServiceCollection serviceCollection)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TIdentifierParams : IRouteParams<TIdentifierParams, TIdentifier>
    where TResponse : class
    {
        serviceCollection
            .AddSingleton<IExternalGateway<TIdentifier, TResponse>, ExternalGateway<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>>()
            ;
    }
}

public class RemoteDddBuilderProxy<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> definition)
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>

{
    public ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> RegisterReadOnly<TIdentifierParams, TResponse>(
        Func<IMapServiceRegisterer<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>, RemoteDddBuilder<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>> factory
    )
    where TResponse : class
    where TIdentifierParams : IRouteParams<TIdentifierParams, TIdentifier>
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.Feature.Extensions.Add(ProxyHelper.RegisterGateway<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>);

        builder.SetEntityReader<ExternalGatewayAdapter<TIdentifier, TEntity, TResponse>, GetEntityUseCase<TIdentifier, TEntity>>();
        builder.SetIdentifierReader<ExternalGatewayAdapter<TIdentifier, TEntity, TResponse>, GetEntityUseCase<TIdentifier, TEntity>>();

        var remoteBuilder = new RemoteDddBuilder<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>(builder, true);
        factory.Invoke(remoteBuilder);

        definition.Features.Add(remoteBuilder.Feature);

        return definition;
    }

    public ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity>
        RegisterStartupRemoteSynchronizationToMemoryRepository<TIdentifierParams, TResponse, TWeight>(
            Func<IMapServiceRegisterer<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>, RemoteDddBuilder<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>> factory
        )
    where TWeight : ISynchronizationOrderByPriority
    where TIdentifierParams : IRouteParams<TIdentifierParams, TIdentifier>
    where TResponse : class
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.Feature.Extensions.Add(
            CommonProxyHelper.RegisterStartupSynchronization<TIdentifier, TEntity, TWeight, ExternalSynchronizationDataSourceAdapter<TIdentifier, TEntity, TResponse>>
        );

        builder.Feature.Extensions.Add(ProxyHelper.RegisterGateway<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>);

        builder.SetRepository<InMemoryDictionaryRepository<TIdentifier, TEntity>>();
        builder.SetEntityReader<TrackedReader<TIdentifier, TEntity>, GetEntityUseCase<TIdentifier, TEntity>>();
        builder.SetIdentifierReader<TrackedReader<TIdentifier, TEntity>, GetEntityUseCase<TIdentifier, TEntity>>();
        builder.SetSpecificationReader<TrackedReader<TIdentifier, TEntity>>();

        var remoteBuilder = new RemoteDddBuilder<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>(builder, false);
        factory.Invoke(remoteBuilder);

        definition.Features.Add(remoteBuilder.Feature);

        return definition;
    }

    public ModuleDefinition<TTag>.RemoteEntityDefinition<TIdentifier, TEntity> Register<TIdentifierParams, TResponse>(
        Func<IMapServiceRegisterer<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>, RemoteDddBuilder<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>>
            factory
    )
    where TIdentifierParams : IRouteParams<TIdentifierParams, TIdentifier>
    where TResponse : class
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.Feature.Extensions.Add(ProxyHelper.RegisterGateway<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>);

        builder.SetEntityReader<ExternalGatewayAdapter<TIdentifier, TEntity, TResponse>, GetEntityUseCase<TIdentifier, TEntity>>();
        builder.SetIdentifierReader<ExternalGatewayAdapter<TIdentifier, TEntity, TResponse>, GetEntityUseCase<TIdentifier, TEntity>>();
        builder.SetEntityRemover<ExternalGatewayAdapter<TIdentifier, TEntity, TResponse>, RemoveEntityUseCase<TIdentifier, TEntity>>();

        var remoteBuilder = new RemoteDddBuilder<TTag, TIdentifierParams, TIdentifier, TEntity, TResponse>(builder, true);
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
