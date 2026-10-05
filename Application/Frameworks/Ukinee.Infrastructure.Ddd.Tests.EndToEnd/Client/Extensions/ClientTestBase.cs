using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Common.Extensions.ServiceCollectionExtensions;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Services;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Extensions;
using Ukinee.Infrastructure.SignalR.Client.Contracts;
using Ukinee.Users.Common.ValueObjects;
using Ukinee.Users.Domain;
using Ukinee.Users.Domain.Contracts;
using Ukinee.Users.Startup;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;

public abstract class ClientTestBase : TestBase
{
    protected readonly MediatRConfig MediatRConfig = new MediatRConfig {
        AssembliesToScanHandlers = [typeof(Program).Assembly],
    };

    protected readonly ClientConfig Config = new ClientConfig {
        ServerBaseAddress = $"https://localhost:{ServerInMemoryConfig.Port}/api/v1/",
    };

    protected readonly UserTokenConfig UserTokenConfig = new UserTokenConfig {
        TokenOptionsSectionName = nameof(TokenOptions),
        TokenOptionsFilePath = "F:\\Files\\Rider\\AspExample\\AspExample\\SecretOptions\\supersecretoptions.json",
    };

    protected readonly RegistrationPolicy RegistrationPolicy = new RegistrationPolicy()
        .AllowMultiple(typeof(IHubConnectionRegisterer<>));

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services
            .AddLogging()
            .SetupCommonServices(MediatRConfig);

        services
            .SetupClientTestingServices(Configuration, UserTokenConfig)
            .SetupClient<TestingTag>(Config, RegistrationPolicy)
            .AddSingleton<TestingUserContextProvider>()
            .AddSingleton<IUserContextProvider>(sp => sp.GetRequiredService<TestingUserContextProvider>());
    }

    protected async Task EnsureToken(UserContext userContext)
    {
        var tokenFactory = GetService<IUserTokenFactory>();
        var tokenStore = GetService<IUserTokenStore>();

        var dummy = User.Dummy;

        var user = dummy with {
            Identifier = UserIdentifier.Create(userContext.Guid),
            Roles = userContext.Roles,
            Account = dummy.Account with {
                Username = userContext.VisibleName,
            },
        };

        var token = tokenFactory.Generate(user);

        tokenStore.SetToken(userContext, token);
    }
}
