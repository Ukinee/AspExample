using Mapster;
using MapsterMapper;
using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.AspNetCore.ModuleRegisterer.Refactor.EntityFeatures;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Implementations;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources.Database;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Postgres.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;
using Ukinee.Infrastructure.Ddd.Local.MediatR;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Services;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Services;
using Ukinee.Infrastructure.Options.Extensions;
using Ukinee.Users.Common.ValueObjects;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Extensions;

public static class SetupMinimalServerExample
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection SetupServer<THub>(IConfigurationManager configuration, ServerConfig config)
        where THub : Hub
        {
            var policy = RegistrationPolicy.Server;

            var mediatRConfig = new MediatRConfig {
                AssembliesToScanHandlers = [typeof(SetupMinimalServerExample).Assembly], // он ОЧЕНЬ хочет сканировать проекты
            };

            SetupMapper(serviceCollection);
            SetupMediatR(serviceCollection, mediatRConfig);
            SetupAuthorization(serviceCollection);
            serviceCollection.AddSignalR();

            serviceCollection.AddSingleton<TimeProvider>(_ => TimeProvider.System);

            serviceCollection
                .AddScoped<UnitOfWorkFactory>()
                .AddScoped<IUnitOfWorkProvider>(sp => sp.GetRequiredService<UnitOfWorkFactory>())
                .AddScoped<IUnitOfWorkFactory>(sp => sp.GetRequiredService<UnitOfWorkFactory>());

            configuration.AddJsonFile(config.ConntectionStringPath, optional: false, reloadOnChange: true);

            serviceCollection
                .AddScoped<IUserContextProvider, TestingUserContextProvider>(_ => new TestingUserContextProvider(UserContext.Test));

            var announcementPolicy = AuthorizationPolicyDefinition.GuestReadAndOwnerEdit<AnnouncementIdentifier, Announcement>(identifier => identifier.UserGuid);
            serviceCollection
                .RegisterModule<ServerTag>()
                .DatabaseDatasource.Register<ServerDbContext>(databaseConfigurator => databaseConfigurator
                    .RegisterPostgresVersioning()
                    .ConfigureContext((_, builder) =>
                        {
                            builder.UseNpgsql(configuration.GetConnectionString(config.DatabaseName));
                        }
                    )
                )
                .LocalContexts.AddHonestDeletionDatabaseContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterWithDatabaseRepository(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(announcementPolicy)
                        .AddCreateUseCase<CreateAnnouncementRequest, AnnouncementFactory>()
                        .AddUpdateUseCase<UpdateAnnouncementContentsRequest, AnnouncementFactory>()
                    )
                    .ApiServer.Register<AnnouncementResponse>(
                        config.ApiBaseRoute,
                        announcementPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .RegisterCrud<CreateAnnouncementRequest, UpdateAnnouncementContentsRequest>()
                            .AddGroupConfiguration(builder => builder.AllowAnonymous())
                    )
                    .SignalRServer.Register<THub, AnnouncementSignalRRequest, AnnouncementResponse>(signalRConfigurator => signalRConfigurator
                        .SetGroupResolver<AnnouncementSignalRRouteResolver>()
                        .SetAccessValidationService<AnnouncementSignalRAccessValidator>()
                    )
                )
                .Build(policy);

            return serviceCollection;
        }
    }

    private static void SetupAuthorization(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddAuthorization(authBuilder =>
                {
                    authBuilder.AddPolicy("LoggedInPolicy", policy => policy.RequireAuthenticatedUser());
                }
            )
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
    }

    public static IServiceCollection SetupMediatR(IServiceCollection serviceCollection, MediatRConfig config)
    {
        serviceCollection
            .AddMediatR(builder =>
                {
                    builder.TypeEvaluator = type => false;
                    builder.Lifetime = ServiceLifetime.Scoped;
                    builder.RegisterGenericHandlers = false;
                    builder.NotificationPublisher = new TaskWhenAllPublisher();
                    builder.AutoRegisterRequestProcessors = false;

                    foreach (var assembly in config.AssembliesToScanHandlers)
                        builder.RegisterServicesFromAssemblies(assembly);
                }
            )
            .Decorate<IMediator, TransactionMediatorDecorator>();

        return serviceCollection;
    }

    private static void SetupMapper(IServiceCollection serviceCollection)
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
