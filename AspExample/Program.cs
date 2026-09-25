using Examples.Client.Domain.Models;
using Examples.Client.Startup;
using Examples.Common.Startup;
using Examples.Server.Infrastructure.Services;
using Examples.Server.Startup;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;
using Ukinee.Users.Domain;
using Ukinee.Users.Startup;

namespace AspExample;

public static class Program
{
    private const string DatabaseOptionsSectionName = "DatabaseOptions";
    private const string SecretOptionsPath = "SecretOptions/supersecretoptions.json";

    public static void Main(string[] args)
    {
        var mode = StartupHelper.GetMode(args);

        var registrationPolicy = CreateRegistrationPolicy();

        var mediatrOptions = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly]
        };

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();

        builder
            .SetupLogging()
            .SetupDevelopmentMiddlewares()
            .SetupRouting()
            .Services
            .SetupCommonServices(mediatrOptions);

        if (!mode.IsServer)
        {
            StartAsClient(builder, mode, registrationPolicy);

            return;
        }

        SetupAsServer(builder, registrationPolicy);

        var app = builder.Build();

        app.RegisterEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.RegisterEndpoints();
        app.MapControllers();

        app.Run($"https://localhost:{mode.Port}");
    }

    private static RegistrationPolicy CreateRegistrationPolicy()
    {
        var registrationPolicy = new RegistrationPolicy();

        registrationPolicy.Ignore<IDisposable>();
        registrationPolicy.AllowMultiple<ApiServerDefinition>();

        return registrationPolicy;
    }

    private static void StartAsClient(WebApplicationBuilder builder, ModeConfig mode, RegistrationPolicy registrationPolicy)
    {
        var clientOptions = new ClientExampleConfig {
            BaseServerAddress = $"https://localhost:{StartupHelper.GetServerConfig().Port}/api/v1",
        };

        builder
            .Services
            .SetupClientTestingServices()
            .SetupClientExample<ClientExampleHubTag>(registrationPolicy, clientOptions);

        var app = builder.Build();
        app.Run($"https://localhost:{mode.Port}");
    }

    private static void SetupAsServer(WebApplicationBuilder builder, RegistrationPolicy registrationPolicy)
    {
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
            .SetupUsers(builder.Configuration, userOwnerConfig)
            .SetupServerExample<ExampleServerHub>(registrationPolicy, serverOptions);
    }
}
