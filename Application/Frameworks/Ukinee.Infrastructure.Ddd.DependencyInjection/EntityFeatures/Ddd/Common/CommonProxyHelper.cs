using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Common.Contracts;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EventBuses.Events;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;
using Ukinee.Infrastructure.Ddd.Synchronization.Implementations;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Common;

public static class CommonProxyHelper
{
    public static void RegisterStartupSynchronization<TIdentifier, TEntity, TWeight, TDataSource>(IServiceCollection serviceCollection)
    where TWeight : ISynchronizationOrderByPriority, allows ref struct
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>
    where TDataSource : class, ISynchronizationDataSource<TEntity>
    {
        serviceCollection
            .AddSingleton<TDataSource>()
            .AddSingleton<ISynchronizationDataSource<TEntity>, TDataSource>()
            .AddSingleton<IOrderedHostedService, SynchronizationHostedService<TEntity>>()
            .AddSingleton<ISynchronizationService<TEntity>, SynchronizationService<TIdentifier, TEntity, TWeight>>();
    }

    public static void RegisterServerNotificationHandler<TIdentifier, TEntity, THandler>(IServiceCollection serviceCollection)
    where TEntity : class, IEntity<TIdentifier>
    where TIdentifier : struct, IEquatable<TIdentifier>
    where THandler : class, INotificationHandler<CreatedDomainEvent<TIdentifier, TEntity>>, INotificationHandler<UpdatedDomainEvent<TIdentifier, TEntity>>, INotificationHandler<DeletedDomainEvent<TIdentifier, TEntity>>
    {
        serviceCollection.AddSingleton<INotificationHandler<CreatedDomainEvent<TIdentifier, TEntity>>, THandler>();
        serviceCollection.AddSingleton<INotificationHandler<UpdatedDomainEvent<TIdentifier, TEntity>>, THandler>();
        serviceCollection.AddSingleton<INotificationHandler<DeletedDomainEvent<TIdentifier, TEntity>>, THandler>();
    }
}
