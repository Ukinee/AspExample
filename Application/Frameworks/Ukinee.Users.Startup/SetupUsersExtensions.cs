using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Ukinee.Infrastructure.Options.Extensions;
using Ukinee.Users.Common.ValueObjects;
using Ukinee.Users.Databases;
using Ukinee.Users.Domain;
using Ukinee.Users.Domain.Contracts;
using Ukinee.Users.Infrastructure.DomainServices;
using Ukinee.Users.Infrastructure.Services;
using TokenOptions = Ukinee.Users.Domain.TokenOptions;

namespace Ukinee.Users.Startup;

public class UserOptionsConfig
{
    public const string SecretPath = "SecretOptions/supersecretoptions.json";
    public const string SectionName = "TokenOptions";

    public required string TokenOptionsSectionName { get; init; }
    public required string TokenOptionsFilePath { get; init; }
}

public class UserConfig : UserOptionsConfig
{
    public required string DatabaseName { get; init; }
}

public static class SetupUsersExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection SetupUsers(IConfigurationManager configuration, UserConfig config)
        {
            serviceCollection
                .AddAuthorization(authBuilder =>
                    {
                        authBuilder.AddPolicy(UserPolicies.AdminPolicy, policy => policy.RequireAuthenticatedUser().RequireRole(UserRoles.Admin));
                        authBuilder.AddPolicy(UserPolicies.LoggedInPolicy, policy => policy.RequireAuthenticatedUser());
                    }
                )
                .ConfigureOptions(configuration, config)
                .ConfigureServices()
                .ConfigureDatabase(config, configuration)
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            return serviceCollection;
        }

        private IServiceCollection ConfigureOptions(IConfigurationManager configuration, UserConfig config)
        {
            serviceCollection.TryAddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

            serviceCollection.ConfigureJson<TokenOptions>(configuration, config.TokenOptionsSectionName, config.TokenOptionsFilePath);

            return serviceCollection;
        }

        private IServiceCollection ConfigureServices()
        {
            serviceCollection.AddSingleton<IUserTokenFactory, TokenFactory>();
            serviceCollection.AddSingleton<IUserFactory, UserFactory>();

            serviceCollection.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

            serviceCollection.AddScoped<UserDatabaseService>();
            serviceCollection.AddScoped<IUserReader>(sp => sp.GetRequiredService<UserDatabaseService>());
            serviceCollection.AddScoped<IUserCreator>(sp => sp.GetRequiredService<UserDatabaseService>());
            serviceCollection.AddScoped<IUserCredentialsUpdater>(sp => sp.GetRequiredService<UserDatabaseService>());

            return serviceCollection;
        }

        private IServiceCollection ConfigureDatabase(UserConfig config, IConfiguration configuration)
        {
            serviceCollection.AddDbContext<UserDbContext>((serviceProvider, builder) =>
                {
                    var connectionString = configuration.GetConnectionString(config.DatabaseName);

                    builder.UseNpgsql(
                        connectionString,
                        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null
                        )
                    );
                }
            );

            return serviceCollection;
        }
    }
}
