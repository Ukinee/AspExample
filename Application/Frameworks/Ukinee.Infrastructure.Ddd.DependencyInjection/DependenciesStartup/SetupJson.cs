using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ukinee.Infrastructure.Json.Models;
using Ukinee.Infrastructure.Json.Utils;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;

public class ConfigureAllApplicationJsonOptions(IEnumerable<PolymorphicMapping> mappings) :
    IConfigureOptions<JsonOptions>,
    IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>,
    IConfigureOptions<JsonHubProtocolOptions>
{
    public void Configure(JsonOptions options) =>
        JsonPolymorphismHelper.ApplyMappings(options.JsonSerializerOptions, mappings);

    public void Configure(Microsoft.AspNetCore.Http.Json.JsonOptions options) =>
        JsonPolymorphismHelper.ApplyMappings(options.SerializerOptions, mappings);

    public void Configure(JsonHubProtocolOptions options) =>
        JsonPolymorphismHelper.ApplyMappings(options.PayloadSerializerOptions, mappings);
}

public static class ConfigureJsonPolymorphism
{
    public static void SetupJson(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ConfigureAllApplicationJsonOptions>();
        serviceCollection.AddSingleton<IConfigureOptions<JsonOptions>>(sp => sp.GetRequiredService<ConfigureAllApplicationJsonOptions>());
        serviceCollection.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>(sp => sp.GetRequiredService<ConfigureAllApplicationJsonOptions>());
        serviceCollection.AddSingleton<IConfigureOptions<JsonHubProtocolOptions>>(sp => sp.GetRequiredService<ConfigureAllApplicationJsonOptions>());
    }
}
