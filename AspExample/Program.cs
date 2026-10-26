using Scalar.AspNetCore;
using Serilog;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Services;
using Ukinee.Infrastructure.Ddd.Tests.Common.Extensions.ServiceCollectionExtensions;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Extensions;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Server.Services;
using Ukinee.Infrastructure.Json.OpenApi;
using Ukinee.Users.Domain;
using Ukinee.Users.Startup;

namespace AspExample;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureAppBuilder(builder);
        ConfigureAppServices(builder);

        var app = builder.Build();

        ConfigureApp(app);

        app.Run($"https://localhost:{ServerConfig.Port}");
    }

    private static void ConfigureAppBuilder(WebApplicationBuilder builder)
    {
      
        builder.Host.UseSerilog((context, dServices, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(dServices)
                    .Enrich.FromLogContext();
            }
        );

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddProblemDetails();
        }
        
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateOnBuild = true; 
            options.ValidateScopes = true;  
        });
    }

    private static void ConfigureApp(WebApplication app)
    {
        var definitions = app.Services.GetServices<ApiServerDefinition>();
        
        foreach (var def in definitions)
        {
            def.RegistrationAction(app);
        }

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

        app.UseExceptionHandler();

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }

    private static void ConfigureAppServices(WebApplicationBuilder builder)
    {
        var serverOptions = new ServerConfig {
            DatabaseName = "Example.Database",
            ApiBaseRoute = "api/v1",
            ConntectionStringPath = "SecretOptions/supersecretoptions.json",
        };

        builder.Services.AddExceptionHandler<DddExceptionsHandler>();
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder
            .Services
            // .SetupServer<ExampleServerHub>(builder.Configuration, serverOptions)
            .SetupTestingServer<ExampleServerHub>(builder.Configuration, serverOptions)
            ;
    }
}
