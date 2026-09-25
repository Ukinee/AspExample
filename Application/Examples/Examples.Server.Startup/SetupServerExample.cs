using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Domain.Presentation.Requests;
using Examples.Common.Domain.Presentation.Responses;
using Examples.Server.Databases;
using Examples.Server.Domain.Models;
using Examples.Server.Domain.Models.Contracts;
using Examples.Server.Infrastructure.DomainServices;
using Examples.Server.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.AspNetCore.ModuleRegisterer.Refactor.EntityFeatures;
using Ukinee.AspNetCore.ModuleRegisterer.Refactor.EntityFeatures.ApiEndpointFeature;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Users.Domain.Contracts;

namespace Examples.Server.Startup;

public class ServerExampleConfig
{
    public required string DatabaseName { get; init; }

    public required string DatabaseOptionsSectionName { get; init; }
    public required string DatabaseOptionsFilePath { get; init; }

    public required string ApiBaseRoute { get; init; }
}

public static class SetupServerExampleExtension
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection SetupServerExample<THub>(RegistrationPolicy policy, ServerExampleConfig config)
        where THub : Hub
        {
            RegisterGeneral(serviceCollection);

            var eventPolicy = AuthorizationPolicy.GuestReadAndOwnerEdit<EventIdentifier, Event>(identifier => identifier.UserGuid);
            var reviewPolicy = AuthorizationPolicy.GuestReadAndOwnerEdit<ReviewIdentifier, Review>(identifier => identifier.ReviewUserGuid);
            var locationPolicy = AuthorizationPolicy.GuestReadAndAdministratorEdit<LocationIdentifier, Location>();
            var locationCategoryPolicy = AuthorizationPolicy.GuestReadAndAdministratorEdit<LocationCategoryIdentifier, LocationCategory>();
            var attendancePolicy = AuthorizationPolicy.OwnerOnly<AttendanceIdentifier, Attendance>(identifier => identifier.AttendeeUserGuid);

            serviceCollection
                .RegisterModule<ServerExampleTag>()
                .DatabaseDatasource.Register<ServerDatabaseContext>((options, builder) =>
                    {
                        var connectionString = options.GetConnectionString(config.DatabaseName);

                        builder.UseNpgsql(
                            connectionString,
                            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorCodesToAdd: null
                            )
                        );
                    }
                )
                .LocalContexts.AddHonestDeletionDatabaseContext<EventIdentifier, Event>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterWithDatabaseRepository(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(eventPolicy)
                        .SetCreateUseCase<CreateEventRequest, CreateEventRequestValidator, EventFactory>()
                        .AddUpdateUseCase<UpdateEventDateRequest, UpdateEventDateRequestValidator, EventFactory>()
                    )
                    .ApiServer.Register<EventIdentifierParams, EventResponse>(
                        config.ApiBaseRoute,
                        (idParams, userContext) => EventIdentifier.Create(userContext.Guid, idParams.EventGuid),
                        eventPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
                            .RegisterCrd<CreateEventRequest>()
                            .AddUpdate<UpdateEventDateRequest>()
                    )
                )
                .LocalContexts.AddSoftDeletionDatabaseContext<ReviewIdentifier, Review>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterWithDatabaseRepository(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(reviewPolicy)
                        .SetCreateUseCase<CreateReviewRequest, CreateReviewRequestValidator, ReviewFactory>()
                    )
                    .ApiServer.Register<ReviewIdentifierParams, ReviewResponse>(
                        config.ApiBaseRoute,
                        (idParams, userContext) => ReviewIdentifier.Create(idParams.EventGuid, idParams.EventUserGuid, userContext.Guid, idParams.ReviewGuid),
                        reviewPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
                            .RegisterCrd<CreateReviewRequest>()
                    )
                    .SignalRServer.Register<THub, SubscribeToReviewsRequest, ReviewResponse>(signalRConfigurator => signalRConfigurator
                        .SetGroupResolver<SubscribeToReviewsRequestGroupResolver>()
                        .SetAccessValidationService<ReviewSignalRAccessValidatorService>()
                    )
                )
                .LocalContexts.AddHonestDeletionDatabaseContext<LocationIdentifier, Location>(contextConfigurator => contextConfigurator
                    .WithServices(ForLocation)
                    .Ddd.RegisterWithDatabaseRepository(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(locationPolicy)
                        .SetCreateUseCase<CreateLocationRequest, CreateLocationRequestValidator, LocationFactory>()
                    )
                    .ApiServer.Register<LocationIdentifierParams, LocationResponse>(
                        config.ApiBaseRoute,
                        (idParams, _) => LocationIdentifier.Create(idParams.LocationHash),
                        locationPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
                            .RegisterCrd<CreateLocationRequest>()
                    )
                )
                .LocalContexts.AddHonestDeletionDatabaseContext<LocationCategoryIdentifier, LocationCategory>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterWithDatabaseRepository(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(locationCategoryPolicy)
                        .SetCreateUseCase<CreateLocationCategoryRequest, CreateLocationCategoryRequestValidator, LocationCategoryFactory>()
                    )
                    .ApiServer.Register<LocationCategoryIdentifierParams, LocationCategoryResponse>(
                        config.ApiBaseRoute,
                        (idParams, _) => LocationCategoryIdentifier.Create(idParams.CategoryName),
                        locationCategoryPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
                            .RegisterCrd<CreateLocationCategoryRequest>()
                    )
                )
                .LocalContexts.AddHonestDeletionDatabaseContext<AttendanceIdentifier, Attendance>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterWithDatabaseRepository(dddConfigurator => dddConfigurator
                        .SetAccessExpressionProvider<AttendanceAccessExpressionProvider>()
                        .SetCreateUseCase<CreateAttendanceRequest, CreateAttendanceRequestValidator, AttendanceFactory>()
                        .AddUpdateUseCase<UpdateAttendanceProbabilityRequest, UpdateAttendanceProbabilityRequestValidator, AttendanceFactory>()
                    )
                    .ApiServer.Register<AttendanceIdentifierParams, AttendanceResponse>(
                        config.ApiBaseRoute,
                        (idParams, userContext) => AttendanceIdentifier.Create(idParams.EventGuid, idParams.EventUserGuid, userContext.Guid),
                        attendancePolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .WithNoAdditionalConfiguration()
                            .RegisterCrd<CreateAttendanceRequest>()
                            .AddUpdate<UpdateAttendanceProbabilityRequest>()
                    )
                )
                .Build(policy);

            return serviceCollection;
        }
    }

    private static void RegisterGeneral(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddScoped<IUserContextProvider, HttpUserContextProvider>();

        serviceCollection.AddSignalR();
    }

    private static void ForLocation(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IAddressHashService, AddressHashService>();
    }
}
