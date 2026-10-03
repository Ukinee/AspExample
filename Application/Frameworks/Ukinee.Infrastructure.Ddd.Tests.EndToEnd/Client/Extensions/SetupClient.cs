using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRClient;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;

public class ClientConfig
{
    public required string ServerBaseAddress { get; init; }
}

public static class SetupClientExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection SetupClient<THub>(ClientConfig clientConfig, RegistrationPolicy registrationPolicy)
        {
            serviceCollection
                .RegisterModule<TestingTag>()
                .ApiDatasource.Register(httpClientBuilder =>
                {
                    httpClientBuilder
                        .ConfigureHttpClient(client =>
                            {
                                client.Timeout = TimeSpan.FromMinutes(3);
                                client.BaseAddress = new Uri(clientConfig.ServerBaseAddress);
                            }
                        )
                        .AddDefaultLogger()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(3));
                })
                .ApiClientContexts.AddRemoteContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
                    .Ddd.Register<AnnouncementIdentifierParams, AnnouncementResponse>(dddConfigurator => dddConfigurator
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateAnnouncementRequest>()
                        .AddUpdateUseCase<UpdateAnnouncementRequest>()
                        .AddLocalCache()
                    )
                    .SignalRClient.Register<THub, AnnouncementSignalRRequest, AnnouncementResponse>(signalRConfigurator => signalRConfigurator
                        .AddLocalCacheUpdate()
                    )
                )
                .ApiClientContexts.AddRemoteContext<BorderIdentifier, Border>(contextConfigurator => contextConfigurator
                    .Ddd.Register<BorderIdentifierParams, BorderResponse>(dddConfigurator => dddConfigurator
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateBorderRequest>()
                        .AddUpdateUseCase<UpdateBorderRequest>()
                        .AddLocalCache()
                    )
                    .SignalRClient.Register<THub, BorderSignalRRequest, BorderResponse>(signalRConfigurator => signalRConfigurator
                        .AddLocalCacheUpdate()
                    )
                )
                .ApiClientContexts.AddRemoteContext<CookieIdentifier, Cookie>(contextConfigurator => contextConfigurator
                    .Ddd.Register<CookieIdentifierParams, CookieResponse>(dddConfigurator => dddConfigurator
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateCookieRequest>()
                        .AddUpdateUseCase<UpdateCookieRequest>()
                        .AddLocalCache()
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
