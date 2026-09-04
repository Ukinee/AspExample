using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCases;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Common;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Repositories;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Local;

file class ProxyHelper
{
    public static void RegisterTrackedReaders<TIdentifier, TEntity>(DddBuilder<TIdentifier, TEntity> builder)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>
    {
        builder.WithEntityReader<TrackedReader<TIdentifier, TEntity>>();
        builder.WithIdentifierReader<TrackedReader<TIdentifier, TEntity>, GetEntityUseCase<TIdentifier, TEntity>>();
        builder.WithSpecificationReader<TrackedReader<TIdentifier, TEntity>>();
    }
}

public class InMemoryDddBuilderProxy<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> definition)
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>
{
    public ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> RegisterAsInMemoryState(
        Func<ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>, TrackedDddBuilder<TTag, TIdentifier, TEntity>> factory
    )
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.WithRepository<InMemoryDictionaryRepository<TIdentifier, TEntity>>();

        ProxyHelper.RegisterTrackedReaders(builder);

        builder.WithDeltaEntityUpdater<TrackedDeltaEntityUpdater<TIdentifier, TEntity>, UpdateEntityUseCase<TIdentifier, TEntity>>();
        builder.WithEntityRemover<HonestTrackedRemover<TIdentifier, TEntity>, RemoveEntityUseCase<TIdentifier, TEntity>>();

        var tackedBuilder = new TrackedDddBuilder<TTag, TIdentifier, TEntity>(builder);

        factory?.Invoke(tackedBuilder);
        definition.Features.Add(tackedBuilder.Feature);

        return definition;
    }

    public ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> RegisterWithLocalStaticDataSource<TWeight, TDataSource>(
        Func<ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>, TrackedDddBuilder<TTag, TIdentifier, TEntity>> factory
    )
    where TWeight : ISynchronizationOrderByPriority, allows ref struct
    where TDataSource : class, ISynchronizationDataSource<TEntity>
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.WithRepository<InMemoryDictionaryRepository<TIdentifier, TEntity>>();
        ProxyHelper.RegisterTrackedReaders(builder);

        var tackedBuilder = new TrackedDddBuilder<TTag, TIdentifier, TEntity>(builder);

        factory?.Invoke(tackedBuilder);
        definition.Features.Add(tackedBuilder.Feature);

        definition.Services.Add(CommonProxyHelper.RegisterStartupSynchronization<TIdentifier, TEntity, TWeight, TDataSource>);

        return definition;
    }
}

public class HonestDeletionDddBuilderProxy<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> definition)
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>
{
    public ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> RegisterWithDatabaseRepository(
        Func<ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>, TrackedDddBuilder<TTag, TIdentifier, TEntity>> factory
    )
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.WithRepository<DbService<TIdentifier, TEntity, TTag>>();
        ProxyHelper.RegisterTrackedReaders(builder);

        builder.WithDeltaEntityUpdater<TrackedDeltaEntityUpdater<TIdentifier, TEntity>, UpdateEntityUseCase<TIdentifier, TEntity>>();
        builder.WithEntityRemover<HonestTrackedRemover<TIdentifier, TEntity>, RemoveEntityUseCase<TIdentifier, TEntity>>();

        var tackedBuilder = new TrackedDddBuilder<TTag, TIdentifier, TEntity>(builder);

        factory?.Invoke(tackedBuilder);
        definition.Features.Add(tackedBuilder.Feature);

        return definition;
    }

    public ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> RegisterWithStartupDbSynchronizationWriteBehindAndInMemoryRepository<TWeight>(
        Func<ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>, TrackedDddBuilder<TTag, TIdentifier, TEntity>> factory
    )
    where TWeight : ISynchronizationOrderByPriority, allows ref struct
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.WithRepository<InMemoryDictionaryRepository<TIdentifier, TEntity>>();
        ProxyHelper.RegisterTrackedReaders(builder);

        builder.WithDeltaEntityUpdater<TrackedDeltaEntityUpdater<TIdentifier, TEntity>, UpdateEntityUseCase<TIdentifier, TEntity>>();
        builder.WithEntityRemover<HonestTrackedRemover<TIdentifier, TEntity>, RemoveEntityUseCase<TIdentifier, TEntity>>();

        definition.Services.Add(CommonProxyHelper.RegisterStartupSynchronization<TIdentifier, TEntity, TWeight, DbService<TIdentifier, TEntity, TTag>>);
        definition.Services.Add(CommonProxyHelper.RegisterNotificationHandler<TIdentifier, TEntity, HonestDatabasePersistenceReceiver<TIdentifier, TEntity, TTag>>);

        var tackedBuilder = new TrackedDddBuilder<TTag, TIdentifier, TEntity>(builder);

        factory?.Invoke(tackedBuilder);
        definition.Features.Add(tackedBuilder.Feature);

        return definition;
    }
}

public class DeletableDddBuilderProxy<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> definition)
where TIdentifier : struct, IEquatable<TIdentifier>
where TEntity : class, IEntity<TIdentifier>, ISpecificationForSoftDelete<TEntity>
{
    public ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> RegisterWithDatabaseRepository(
        Func<ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>, TrackedDddBuilder<TTag, TIdentifier, TEntity>> factory
    )
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.WithRepository<DbService<TIdentifier, TEntity, TTag>>();
        builder.WithEntityReader<TrackedReader<TIdentifier, TEntity>>();
        builder.WithIdentifierReader<TrackedReader<TIdentifier, TEntity>, GetEntityUseCase<TIdentifier, TEntity>>();
        builder.WithSpecificationReader<TrackedReader<TIdentifier, TEntity>>();

        builder.WithDeltaEntityUpdater<TrackedDeltaEntityUpdater<TIdentifier, TEntity>, UpdateEntityUseCase<TIdentifier, TEntity>>();
        builder.WithEntityRemover<DeletableTrackedRemover<TIdentifier, TEntity>, RemoveEntityUseCase<TIdentifier, TEntity>>();

        var tackedBuilder = new TrackedDddBuilder<TTag, TIdentifier, TEntity>(builder);

        factory?.Invoke(tackedBuilder);
        definition.Features.Add(tackedBuilder.Feature);

        return definition;
    }

    public ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> RegisterWithStartupDbSynchronizationWriteBehindAndInMemoryRepository<TWeight>(
        Func<ITrackedDddIdentifierAccessValidatorBuilder<TTag, TIdentifier, TEntity>, TrackedDddBuilder<TTag, TIdentifier, TEntity>> factory
    )
    where TWeight : ISynchronizationOrderByPriority, allows ref struct
    {
        var feature = DddFeature<TIdentifier, TEntity>.Empty;
        var builder = new DddBuilder<TIdentifier, TEntity>(feature);

        builder.WithRepository<InMemoryDictionaryRepository<TIdentifier, TEntity>>();
        builder.WithEntityReader<TrackedReader<TIdentifier, TEntity>>();
        builder.WithIdentifierReader<TrackedReader<TIdentifier, TEntity>, GetEntityUseCase<TIdentifier, TEntity>>();
        builder.WithSpecificationReader<TrackedReader<TIdentifier, TEntity>>();

        builder.WithDeltaEntityUpdater<TrackedDeltaEntityUpdater<TIdentifier, TEntity>, UpdateEntityUseCase<TIdentifier, TEntity>>();
        builder.WithEntityRemover<DeletableTrackedRemover<TIdentifier, TEntity>, RemoveEntityUseCase<TIdentifier, TEntity>>();

        definition.Services.Add(CommonProxyHelper.RegisterStartupSynchronization<TIdentifier, TEntity, TWeight, DbService<TIdentifier, TEntity, TTag>>);
        definition.Services.Add(CommonProxyHelper.RegisterNotificationHandler<TIdentifier, TEntity, DeletableDatabasePersistenceReceiver<TIdentifier, TEntity, TTag>>);

        var tackedBuilder = new TrackedDddBuilder<TTag, TIdentifier, TEntity>(builder);

        factory?.Invoke(tackedBuilder);
        definition.Features.Add(tackedBuilder.Feature);

        return definition;
    }
}

public static class LocalEntityDefinitionExtensions
{
    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.InMemoryEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public InMemoryDddBuilderProxy<TTag, TIdentifier, TEntity> Ddd => new InMemoryDddBuilderProxy<TTag, TIdentifier, TEntity>(definition);
    }

    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.HonestDeletionEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>
    {
        public HonestDeletionDddBuilderProxy<TTag, TIdentifier, TEntity> Ddd => new HonestDeletionDddBuilderProxy<TTag, TIdentifier, TEntity>(definition);
    }

    extension<TTag, TIdentifier, TEntity>(ModuleDefinition<TTag>.SoftDeletionEntityDefinition<TIdentifier, TEntity> definition)
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TEntity : class, IEntity<TIdentifier>, ISpecificationForSoftDelete<TEntity>
    {
        public DeletableDddBuilderProxy<TTag, TIdentifier, TEntity> Ddd => new DeletableDddBuilderProxy<TTag, TIdentifier, TEntity>(definition);
    }
}
