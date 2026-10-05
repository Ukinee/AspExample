using System.Text.Json.Serialization;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Common.Contracts;
using Ukinee.Infrastructure.Common.Services;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.External.Api.Domain.ValueObjects;
using Ukinee.Infrastructure.Json.Extensions.ServiceCollections;
using Ukinee.Infrastructure.Json.Services;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Extensions.ServiceCollectionExtensions;

public static class SetupCommon
{
    public static IServiceCollection SetupCommonServices(this IServiceCollection serviceCollection, MediatRConfig config)
    {
        SetupServices(serviceCollection);
        RegisterControllers(serviceCollection);
        SetupMapster(serviceCollection);

        serviceCollection.SetupJson();
        serviceCollection.SetupUnitOfWork();
        serviceCollection.SetupMediatR(config);

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
        
        serviceCollection.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringFlagsEnumConverterFactory());
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.Converters.Add(new DynamicPayloadConverter());
                options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
            }
        );
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
}
