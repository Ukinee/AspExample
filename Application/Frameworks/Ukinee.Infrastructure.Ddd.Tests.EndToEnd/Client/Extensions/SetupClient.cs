using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Options;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRClient;
using Ukinee.Infrastructure.Ddd.External.Api.Services;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;
using Ukinee.Infrastructure.Options.Extensions;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;

public class ClientConfig
{
    public required string ServerBaseAddress { get; init; }

    public required string CacheOptionsSectionName { get; init; }
    public required string CacheOptionsFilePath { get; init; }
}

public static class SetupClientExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection SetupClient<THub>(IConfigurationManager configuration, ClientConfig config, RegistrationPolicy registrationPolicy)
        {
            serviceCollection.AddTransient<UserTokenCredentialsHttpInterceptor>();
            
            serviceCollection.ConfigureJson<CachingOptions>(configuration, config.CacheOptionsSectionName, config.CacheOptionsFilePath);

            serviceCollection
                .RegisterModule<TestingTag>()
                .ApiDatasource.Register(httpClientBuilder =>
                    {
                        httpClientBuilder
                            .ConfigureHttpClient(client =>
                                {
                                    client.Timeout = TimeSpan.FromMinutes(3);
                                    client.BaseAddress = new Uri(config.ServerBaseAddress);
                                }
                            )
                            .AddHttpMessageHandler<UserTokenCredentialsHttpInterceptor>()
                            .AddDefaultLogger()
                            .SetHandlerLifetime(TimeSpan.FromMinutes(3));
                    }
                )
                .ApiClientContexts.AddRemoteContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
                    .Ddd.Register<AnnouncementIdentifierParams, AnnouncementResponse>(dddConfigurator => dddConfigurator
                        .WithCache(CachingVariant.UpdateInvalidates)
                        .WithMapper<AnnouncementMapService>()
                        .SetCreateUseCase<CreateAnnouncementRequest>()
                        .AddUpdateUseCase<UpdateAnnouncementRequest>()
                    )
                    .SignalRClient.Register<THub, AnnouncementSignalRRequest, AnnouncementResponse>(signalRConfigurator => signalRConfigurator
                        .AddLocalCacheUpdate()
                    )
                )
                .ApiClientContexts.AddRemoteContext<BorderIdentifier, Border>(contextConfigurator => contextConfigurator
                    .Ddd.Register<BorderIdentifierParams, BorderResponse>(dddConfigurator => dddConfigurator
                        .WithCache(CachingVariant.UpdateInvalidates)
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateBorderRequest>()
                        .AddUpdateUseCase<UpdateBorderRequest>()
                    )
                    .SignalRClient.Register<THub, BorderSignalRRequest, BorderResponse>(signalRConfigurator => signalRConfigurator
                        .AddLocalCacheUpdate()
                    )
                )
                .ApiClientContexts.AddRemoteContext<CookieIdentifier, Cookie>(contextConfigurator => contextConfigurator
                    .Ddd.Register<CookieIdentifierParams, CookieResponse>(dddConfigurator => dddConfigurator
                        .WithCache(CachingVariant.UpdateInvalidates)
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateCookieRequest>()
                        .AddUpdateUseCase<UpdateCookieRequest>()
                    )
                    .SignalRClient.Register<THub, BorderSignalRRequest, BorderResponse>(signalRConfigurator => signalRConfigurator
                        .AddLocalCacheUpdate()
                    )
                )
                .Build(registrationPolicy);

            return serviceCollection;
        }
    }
}
