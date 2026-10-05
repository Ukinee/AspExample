using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.External.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Services;
using Ukinee.Infrastructure.Options.Extensions;
using Ukinee.Users.Domain;
using Ukinee.Users.Domain.Contracts;
using Ukinee.Users.Infrastructure.Services;
using Ukinee.Users.Startup;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;

public static class SetupClientTestingServicesExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection SetupClientTestingServices(IConfigurationManager configuration, UserOptionsConfig config)
        {
            services.AddSingleton<IUserTokenFactory, TokenFactory>();
            services.AddSingleton<IUserTokenStore, TestingUserTokenStore>();

            services.ConfigureJson<TokenOptions>(configuration, config.TokenOptionsSectionName, config.TokenOptionsFilePath);

            return services;
        }
    }
}
