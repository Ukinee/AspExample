using System.Reflection;
using System.Text.Json.Serialization;
using Mapster;
using MapsterMapper;
using MediatR.NotificationPublishers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Ukinee.Infrastructure.Common.Contracts;
using Ukinee.Infrastructure.Common.Services;
using Ukinee.Infrastructure.Ddd.DependencyInjection;
using Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;
using Ukinee.Infrastructure.Json.Extensions.ServiceCollections;
using Ukinee.Infrastructure.Json.Services;

namespace Examples.Common.Startup;

public class MediatRConfig
{
    public required IReadOnlyCollection<Assembly> AssembliesToScanHandlers { get; init; }
}

public static class SetupExampleCommon
{
    public static IServiceCollection SetupCommonServices(this IServiceCollection serviceCollection, MediatRConfig config)
    {
        SetupServices(serviceCollection);
        SetupJson(serviceCollection);
        RegisterControllers(serviceCollection);
        SetupMapster(serviceCollection);
        SetupMediatR(serviceCollection, config);

        return serviceCollection;
    }

    private static void SetupServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService<OrderedHostedServiceStarter>();

        serviceCollection.AddSingleton<IFileHashService, FileHashService>();
        serviceCollection.AddSingleton<IApplicationPathProviderService, ApplicationPathProviderService>();
        
        serviceCollection.AddSingleton<TimeProvider>(_ => TimeProvider.System);

        serviceCollection.AddJsonProvider<ExternalGatewayTag>(options =>
            {
                options.PropertyNameCaseInsensitive = true;
            }
        );
    }

    private static void SetupJson(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ConfigureAllApplicationJsonOptions>();
        serviceCollection.AddSingleton<IConfigureOptions<JsonOptions>>(sp => sp.GetRequiredService<ConfigureAllApplicationJsonOptions>());
        serviceCollection.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>(sp => sp.GetRequiredService<ConfigureAllApplicationJsonOptions>());
        serviceCollection.AddSingleton<IConfigureOptions<JsonHubProtocolOptions>>(sp => sp.GetRequiredService<ConfigureAllApplicationJsonOptions>());
    }

    private static void RegisterControllers(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddControllers()
            .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringFlagsEnumConverterFactory());
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new DynamicPayloadConverter());
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
                }
            )
            .AddControllersAsServices();
    }

    private static void SetupMapster(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<TypeAdapterConfig>(_ =>
                {
                    var config = TypeAdapterConfig.GlobalSettings;

                    config.Default.RequireDestinationMemberSource(true);
                    config.Compile(true);

                    return config;
                }
            )
            .AddSingleton<IMapper, ServiceMapper>();
    }

    private static void SetupMediatR(IServiceCollection serviceCollection, MediatRConfig config)
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
    }
}
