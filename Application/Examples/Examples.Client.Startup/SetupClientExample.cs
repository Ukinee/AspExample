using Examples.Client.Domain.Models;
using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Domain.Presentation.Requests;
using Examples.Common.Domain.Presentation.Responses;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.SignalRClient;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Examples.Client.Startup;

public class ClientExampleConfig
{
    public required string BaseServerAddress { get; init; }
}

public static class SetupClientExampleExtension
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection SetupClientExample<THubTag>(RegistrationPolicy policy, ClientExampleConfig options)
        {
            serviceCollection
                .RegisterModule<ClientExampleTag>()
                .ApiDatasource.Register(httpClientBuilder =>
                    {
                        httpClientBuilder
                            .ConfigureHttpClient(client =>
                                {
                                    client.Timeout = TimeSpan.FromMinutes(3);
                                    client.BaseAddress = new Uri(options.BaseServerAddress);
                                }
                            )
                            .AddDefaultLogger()
                            .SetHandlerLifetime(TimeSpan.FromMinutes(3));
                    }
                )
                .ApiClientContexts.AddRemoteContext<EventIdentifier, Event>(contextConfigurator => contextConfigurator
                    .Ddd.Register<EventIdentifierParams, EventResponse>(dddConfigurator => dddConfigurator
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateEventRequest>()
                        .AddUpdateUseCase<UpdateEventDateRequest>()
                        .AddLocalCache()
                    )
                )
                .ApiClientContexts.AddRemoteContext<ReviewIdentifier, Review>(contextConfigurator => contextConfigurator
                    .Ddd.Register<ReviewIdentifierParams, ReviewResponse>(dddConfigurator => dddConfigurator
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateReviewRequest>()
                        .AddLocalCache()
                    )
                    .SignalRClient.Register<THubTag, SubscribeToReviewsRequest, ReviewResponse>(signalRConfigurator =>
                        signalRConfigurator.AddLocalCacheUpdate()
                    )
                )
                .ApiClientContexts.AddRemoteContext<LocationIdentifier, Location>(contextConfigurator => contextConfigurator
                    .Ddd.Register<LocationIdentifierParams, LocationResponse>(dddConfigurator => dddConfigurator
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateLocationRequest>()
                        .AddLocalCache()
                    )
                )
                .ApiClientContexts.AddRemoteContext<LocationCategoryIdentifier, LocationCategory>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterStartupRemoteSynchronizationToMemoryRepository<LocationCategoryIdentifierParams, LocationCategoryResponse,
                        SynchronizationOrderByPriority40>(dddConfigurator => dddConfigurator
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateLocationCategoryRequest>()
                    )
                )
                .ApiClientContexts.AddRemoteContext<AttendanceIdentifier, Attendance>(contextConfigurator => contextConfigurator
                    .Ddd.Register<AttendanceIdentifierParams, AttendanceResponse>(dddConfigurator => dddConfigurator
                        .WithMapsterResponseToEntityMap()
                        .SetCreateUseCase<CreateAttendanceRequest>()
                        .AddUpdateUseCase<UpdateAttendanceProbabilityRequest>()
                        .AddLocalCache()
                    )
                )
                .Build(policy);

            return serviceCollection;
        }
    }
}
