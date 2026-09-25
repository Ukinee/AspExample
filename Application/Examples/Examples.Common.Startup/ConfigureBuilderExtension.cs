using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Examples.Common.Startup;

public static class ConfigureBuilderExtension
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder SetupLogging()
        {
            builder.Host.UseSerilog((context, dServices, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(builder.Configuration)
                        .ReadFrom.Services(dServices)
                        .Enrich.FromLogContext();
                }
            );

            return builder;
        }

        public WebApplicationBuilder SetupDevelopmentMiddlewares()
        {
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddProblemDetails();
            }

            return builder;
        }

        public WebApplicationBuilder SetupRouting()
        {
            builder.Services.AddRouting(routeOptions =>
                {
                    routeOptions.LowercaseUrls = true;
                    routeOptions.LowercaseQueryStrings = true;
                }
            );

            return builder;
        }
    }
}
