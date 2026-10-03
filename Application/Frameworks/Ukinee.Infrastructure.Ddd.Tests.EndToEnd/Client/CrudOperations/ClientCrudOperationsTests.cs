using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.Tests.Common.Extensions.ServiceCollectionExtensions;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Services;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Extensions;
using Ukinee.Infrastructure.SignalR.Client.Contracts;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.CrudOperations;

public class ClientCrudOperationsTests : TestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var mediatRConfig = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly],
        };

        var config = new ClientConfig {
            ServerBaseAddress = $"https://localhost:{ServerInMemoryConfig.Port}/api/v1",
        };

        var registrationPolicy = new RegistrationPolicy().AllowMultiple(typeof(IHubConnectionRegisterer<>));

        services
            .AddLogging()
            .SetupUnitOfWork()
            .SetupCommonServices(mediatRConfig)
            ;

        services
            .SetupClient<TestingTag>(config, registrationPolicy)
            .AddSingleton<TestingUserContextProvider>()
            .AddSingleton<IUserContextProvider>(sp => sp.GetRequiredService<TestingUserContextProvider>())
            ;
    }

    [Test]
    public async Task CreateUseCase_LoggedIn_ShouldCreateAvailableToGetEntityOnBacked()
    {
        var userProvider = GetService<TestingUserContextProvider>();
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var getUseCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();
        
    }
}
