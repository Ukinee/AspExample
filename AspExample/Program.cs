using Scalar.AspNetCore;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;
using Ukinee.Infrastructure.Ddd.Tests.Common.Extensions.ServiceCollectionExtensions;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Extensions;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Services;
using Ukinee.Infrastructure.Json.OpenApi;
using Ukinee.Users.Domain;
using Ukinee.Users.Startup;

namespace AspExample;

public static class Program
{
    private const string DatabaseOptionsSectionName = "DatabaseOptions";
    private const string SecretOptionsPath = "SecretOptions/supersecretoptions.json";

    public static void Main(string[] args)
    {
        var registrationPolicy = CreateRegistrationPolicy();

        var builder = WebApplication.CreateBuilder(args);

        ConfigureAppBuilder(builder);
        ConfigureAppServices(builder, registrationPolicy);

        var app = builder.Build();

        ConfigureApp(app);

        app.Run($"https://localhost:{ServerInMemoryConfig.Port}");
    }

    private static void ConfigureAppBuilder(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi(options =>
            {
                JsonOpenApiConfigurator.FixFlagEnums(options); //requires JsonStringFlagsEnumConverterFactory registered in middleware pipeline
            }
        );

        builder
            .SetupLogging()
            .SetupDevelopmentMiddlewares()
            .SetupRouting();
    }

    private static void ConfigureApp(WebApplication app)
    {
        app.RegisterEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();

            app.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle("API v1")
                        .WithTheme(ScalarTheme.Saturn)
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                }
            );
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }

    private static void ConfigureAppServices(WebApplicationBuilder builder, RegistrationPolicy registrationPolicy)
    {
        var mediatrOptions = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly]
        };

        var serverOptions = new ServerExampleConfig {
            DatabaseName = "Example.Database",
            DatabaseOptionsSectionName = DatabaseOptionsSectionName,
            DatabaseOptionsFilePath = SecretOptionsPath,
            ApiBaseRoute = "api/v1",
        };

        var userOwnerConfig = new UserConfig {
            DatabaseName = "Example.Database",
            DatabaseOptionsSectionName = DatabaseOptionsSectionName,
            DatabaseOptionsFilePath = SecretOptionsPath,
            TokenOptionsSectionName = nameof(TokenOptions),
            TokenOptionsFilePath = SecretOptionsPath,
        };

        builder
            .Services
            .SetupCommonServices(mediatrOptions)
            .SetupUsers(builder.Configuration, userOwnerConfig)
            .SetupInMemoryServer<ExampleServerHub>(registrationPolicy, serverOptions)
            .SetupUnitOfWork();
    }

    private static RegistrationPolicy CreateRegistrationPolicy()
    {
        var registrationPolicy = new RegistrationPolicy();

        registrationPolicy.Ignore<IDisposable>();
        registrationPolicy.AllowMultiple<ApiServerDefinition>();

        return registrationPolicy;
    }
}
