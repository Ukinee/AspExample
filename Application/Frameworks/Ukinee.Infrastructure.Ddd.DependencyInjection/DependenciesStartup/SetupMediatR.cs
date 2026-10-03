using System.Reflection;
using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Local.MediatR;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;

public class MediatRConfig
{
    public required IReadOnlyCollection<Assembly> AssembliesToScanHandlers { get; init; }
}

public static class SetupMediatRExtension
{
    public static IServiceCollection SetupMediatR(this IServiceCollection serviceCollection, MediatRConfig config)
    {
        serviceCollection.AddMediatR(builder =>
            {
                builder.TypeEvaluator = type => false;
                builder.Lifetime = ServiceLifetime.Singleton;
                builder.RegisterGenericHandlers = false;
                builder.NotificationPublisher = new TaskWhenAllPublisher();
                builder.AutoRegisterRequestProcessors = false;

                foreach (var assembly in config.AssembliesToScanHandlers)
                    builder.RegisterServicesFromAssemblies(assembly);
            }
        );

        serviceCollection.Decorate<IMediator, TransactionMediatorDecorator>();

        return serviceCollection;
    }
}
