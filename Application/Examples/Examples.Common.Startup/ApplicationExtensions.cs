using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.ApiServer;

namespace Examples.Common.Startup;

public static class ApplicationExtensions
{
    public static WebApplication RegisterEndpoints(this WebApplication app)
    {
        var definitions = app.Services.GetServices<ApiServerDefinition>();
        
        foreach (var def in definitions)
        {
            def.RegistrationAction(app);
        }
        
        return app;
    }
}
