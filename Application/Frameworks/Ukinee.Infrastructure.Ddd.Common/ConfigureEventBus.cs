using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.Common;

public static class ServiceCollectionExtensions
{
    public static void AddMediatR(this IServiceCollection services, IEnumerable<Type> typesToScan)
    {
        services.AddMediatR(builder =>
            {
                builder.TypeEvaluator = type => false;
                builder.Lifetime = ServiceLifetime.Singleton;
                builder.RegisterGenericHandlers = false;
                builder.NotificationPublisher = new TaskWhenAllPublisher();

                foreach (var type in typesToScan)
                {
                    builder.RegisterServicesFromAssemblyContaining(type);
                }
            }
        );
    }
}
