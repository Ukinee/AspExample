using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.AspNetCore.ModuleRegisterer.Refactor.EntityFeatures;
using Ukinee.AspNetCore.ModuleRegisterer.Refactor.EntityFeatures.ApiEndpointFeature;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Services;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Extensions;

public class ServerTag;

public class ServerInMemoryConfig
{
    public const string Port = "5002";
    
    public required string ApiBaseRoute { get; init; }
}

public class ServerExampleConfig : ServerInMemoryConfig
{
    public required string DatabaseName { get; init; }

    public required string DatabaseOptionsSectionName { get; init; }
    public required string DatabaseOptionsFilePath { get; init; }
}

public static class SetupServerServices
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection SetupInMemoryServer<THub>(RegistrationPolicy policy, ServerInMemoryConfig config)
        where THub : Hub
        {
            serviceCollection
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddScoped<IUserContextProvider, HttpUserContextProvider>();
            
            RegisterGeneral(serviceCollection);

            var announcementPolicy = AuthorizationPolicyDefinition.GuestReadAndOwnerEdit<AnnouncementIdentifier, Announcement>(identifier => identifier.UserGuid);
            var borderPolicy = AuthorizationPolicyDefinition.AdministratorOnly<BorderIdentifier, Border>();
            var cookiePolicy = TestingAuthorizationPolicyDefinition.VipOwnerOnly<CookieIdentifier, Cookie>(identifier => identifier.UserGuid);

            serviceCollection
                .RegisterModule<ServerTag>()
                .LocalContexts.AddInMemoryContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(announcementPolicy)
                        .SetCreateUseCase<CreateAnnouncementRequest, CreateAnnouncementRequestValidator, AnnouncementFactory>()
                        .AddUpdateUseCase<UpdateAnnouncementContentsRequest, UpdateAnnouncementContentsRequestValidator, AnnouncementFactory>()
                    )
                    .ApiServer.Register<AnnouncementResponse>(
                        config.ApiBaseRoute,
                        announcementPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
                            .RegisterCrud<CreateAnnouncementRequest, UpdateAnnouncementContentsRequest>()
                    )
                    .SignalRServer.Register<THub, AnnouncementSignalRRequest, AnnouncementResponse>(signalRConfigurator => signalRConfigurator
                        .SetGroupResolver<AnnouncementSignalRRouteResolver>()
                        .SetAccessValidationService<AnnouncementSignalRAccessValidator>()
                    )
                )
                .LocalContexts.AddHonestDeletionDatabaseContext<BorderIdentifier, Border>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterWithDatabaseRepository(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(borderPolicy)
                        .SetCreateUseCase<CreateBorderRequest, CreateBorderRequestValidator, BorderFactory>()
                        .AddUpdateUseCase<UpdateBorderRequest, UpdateBorderRequestValidator, BorderFactory>()
                    )
                    .ApiServer.Register<BorderResponse>(
                        config.ApiBaseRoute,
                        borderPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
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
                        .SetCreateUseCase<CreateCookieRequest, CreateCookieRequestValidator, CookieFactory>()
                        .AddUpdateUseCase<UpdateCookieRequest, UpdateCookieRequestValidator, CookieFactory>()
                    )
                    .ApiServer.Register<CookieResponse>(
                        config.ApiBaseRoute,
                        cookiePolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
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

    private static void RegisterGeneral(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSignalR();
    }
}
