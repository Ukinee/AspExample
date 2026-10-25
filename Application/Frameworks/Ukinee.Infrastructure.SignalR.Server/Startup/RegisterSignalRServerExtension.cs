using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Infrastructure.SignalR.Server.Domain;
using Ukinee.Infrastructure.SignalR.Server.Services;
using Ukinee.Infrastructure.SignalR.Server.UseCases;

namespace Ukinee.Infrastructure.SignalR.Server.Startup;

public static class RegisterSignalRServerExtension
{
    public static IServiceCollection RegisterLocalSignalRServer<THub>(this IServiceCollection services)
    where THub : HubBase
    {
        services.AddSignalR();

        services
            .AddSingleton<ISignalRConnectionStateStore, LocalSignalRConnectionStateStore>()
            .AddSingleton<IAbortSignalRConnectionUseCase, LocalAbortSignalRConnectionUseCase>()
            ;

        services
            .AddSingleton<HubSessionStore>()
            .AddTransient<INotificationHandler<UserContextChangedNotification>, UserContextChangedNotificationHandler<THub>>()
            ;

        return services;
    }
}
