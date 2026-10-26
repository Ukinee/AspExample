using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Ukinee.AspNetCore.ModuleRegisterer.Refactor.EntityFeatures;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;
using Ukinee.Infrastructure.Ddd.Tests.Common.Extensions.ServiceCollectionExtensions;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Services;
using Ukinee.Infrastructure.SignalR.Server.Services;
using Ukinee.Infrastructure.SignalR.Server.Startup;
using Ukinee.Users.Domain.Contracts;
using Ukinee.Users.Startup;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Extensions;

public class ServerTag;

public class ServerConfig
{
    public const string Port = "5002";

    public required string ApiBaseRoute { get; init; }

    public required string DatabaseName { get; init; }
    public required string ConntectionStringPath { get; set; }
}

public static class SetupServerServices
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection SetupTestingServer<THub>(IConfigurationManager configuration, ServerConfig config)
        where THub : HubBase
        {
            var mediatrOptions = new MediatRConfig {
                AssembliesToScanHandlers = [typeof(SetupServerServices).Assembly]
            };

            var serConfig = new UserConfig {
                TokenOptionsSectionName = UserOptionsConfig.SectionName,
                TokenOptionsFilePath = UserOptionsConfig.SecretPath,
                DatabaseName = config.DatabaseName
            };

            var policy = RegistrationPolicy.Server;

            serviceCollection.SetupCommonServices(mediatrOptions);
            serviceCollection.SetupUsers(configuration, serConfig);

            serviceCollection
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddScoped<IUserContextProvider, HttpUserContextProvider>();

            serviceCollection.RegisterLocalSignalRServer<THub>();

            var announcementPolicy = AuthorizationPolicyDefinition.GuestReadAndOwnerEdit<AnnouncementIdentifier, Announcement>(identifier => identifier.UserGuid);
            var borderPolicy = AuthorizationPolicyDefinition.AdministratorOnly<BorderIdentifier, Border>();
            var cookiePolicy = TestingAuthorizationPolicyDefinition.VipOwnerOnly<CookieIdentifier, Cookie>(identifier => identifier.UserGuid);

            serviceCollection
                .RegisterModule<ServerTag>()
                .LocalContexts.AddInMemoryContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(announcementPolicy)
                        .AddCreateUseCase<CreateAnnouncementRequest, CreateAnnouncementRequestValidator, AnnouncementFactory>()
                        .AddUpdateUseCase<UpdateAnnouncementContentsRequest, UpdateAnnouncementContentsRequestValidator, AnnouncementFactory>()
                    )
                    .ApiServer.Register<AnnouncementResponse>(
                        config.ApiBaseRoute,
                        announcementPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .RegisterCrud<CreateAnnouncementRequest, UpdateAnnouncementContentsRequest>()
                    )
                    .SignalRServer.Register<THub, AnnouncementSignalRRequest, AnnouncementResponse>(signalRConfigurator => signalRConfigurator
                        .SetGroupResolver<AnnouncementSignalRRouteResolver>()
                        .SetAccessValidationService<AnnouncementSignalRAccessValidator>()
                    )
                )
                .LocalContexts.AddInMemoryContext<BorderIdentifier, Border>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(borderPolicy)
                        .AddCreateUseCase<CreateBorderRequest, CreateBorderRequestValidator, BorderFactory>()
                        .AddUpdateUseCase<UpdateBorderRequest, UpdateBorderRequestValidator, BorderFactory>()
                    )
                    .ApiServer.Register<BorderResponse>(
                        config.ApiBaseRoute,
                        borderPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .RegisterCrud<CreateBorderRequest, UpdateBorderRequest>()
                    )
                    .SignalRServer.Register<THub, BorderSignalRRequest, BorderResponse>(signalRConfigurator => signalRConfigurator
                        .SetGroupResolver<BorderSignalRRouteResolver>()
                        .SetAccessValidationService<BorderSignalRAccessValidator>()
                    )
                )
                .LocalContexts.AddInMemoryContext<CookieIdentifier, Cookie>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(cookiePolicy)
                        .AddCreateUseCase<CreateCookieRequest, CreateCookieRequestValidator, CookieFactory>()
                        .AddUpdateUseCase<UpdateCookieRequest, UpdateCookieRequestValidator, CookieFactory>()
                    )
                    .ApiServer.Register<CookieResponse>(
                        config.ApiBaseRoute,
                        cookiePolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .RegisterCrud<CreateCookieRequest, UpdateCookieRequest>()
                    )
                    .SignalRServer.Register<THub, CookieSignalRRequest, CookieResponse>(signalRConfigurator => signalRConfigurator
                        .SetGroupResolver<CookieSignalRRouteResolver>()
                        .SetAccessValidationService<CookieSignalRAccessValidator>()
                    )
                )
                .Build(policy);

            return serviceCollection;
        }
    }
}
