using Examples.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Common.Contracts;
using Ukinee.Users.Domain.Contracts;
using Ukinee.Users.Infrastructure.Implementations;

namespace Examples.Common.Startup;

public static class SetupTestingServicesExtensions
{
    public static IServiceCollection SetupClientTestingServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUserContextProvider, TestingUserContextProvider>();
        serviceCollection.AddSingleton<IOrderedHostedService, TestingHostedService>();

        return serviceCollection;
    }
}
