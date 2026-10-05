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
                .RegisterModule<TestingTag>()
                .LocalContexts.AddInMemoryContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(announcementPolicy)
                        .SetCreateUseCase<CreateAnnouncementRequest, CreateAnnouncementRequestValidator, AnnouncementFactory>()
                        .AddUpdateUseCase<UpdateAnnouncementRequest, UpdateAnnouncementRequestValidator, AnnouncementFactory>()
                    )
                    .ApiServer.Register<AnnouncementIdentifierParams, AnnouncementResponse>(
                        config.ApiBaseRoute,
                        (idParams, _) => idParams.ToIdentifier(),
                        announcementPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
                            .RegisterCrd<CreateAnnouncementRequest>()
                            .RegisterUpdate<UpdateAnnouncementRequest>()
                    )
                    .SignalRServer.Register<THub, AnnouncementSignalRRequest, AnnouncementResponse>(signalRConfigurator => signalRConfigurator
                        .SetGroupResolver<AnnouncementSignalRRouteResolver>()
                        .SetAccessValidationService<AnnouncementSignalRAccessValidator>()
                    )
                )
                .LocalContexts.AddInMemoryContext<BorderIdentifier, Border>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(borderPolicy)
                        .SetCreateUseCase<CreateBorderRequest, CreateBorderRequestValidator, BorderFactory>()
                        .AddUpdateUseCase<UpdateBorderRequest, UpdateBorderRequestValidator, BorderFactory>()
                    )
                    .ApiServer.Register<BorderIdentifierParams, BorderResponse>(
                        config.ApiBaseRoute,
                        (idParams, _) => idParams.ToIdentifier(),
                        borderPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
                            .RegisterCrd<CreateBorderRequest>()
                            .RegisterUpdate<UpdateBorderRequest>()
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
                    .ApiServer.Register<CookieIdentifierParams, CookieResponse>(
                        config.ApiBaseRoute,
                        (idParams, _) => idParams.ToIdentifier(),
                        cookiePolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
                            .RegisterCrd<CreateCookieRequest>()
                            .RegisterUpdate<UpdateCookieRequest>()
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
