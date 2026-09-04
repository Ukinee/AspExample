using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Ukinee.Infrastructure.Json.Models;
using Ukinee.Infrastructure.Json.Utils;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection;

public class ConfigureAllApplicationJsonOptions(IEnumerable<PolymorphicMapping> mappings) : 
    IConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions>,
    IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>,
    IConfigureOptions<JsonHubProtocolOptions>
{
    public void Configure(Microsoft.AspNetCore.Mvc.JsonOptions options) 
        => JsonPolymorphismHelper.ApplyMappings(options.JsonSerializerOptions, mappings);

    public void Configure(Microsoft.AspNetCore.Http.Json.JsonOptions options) 
        => JsonPolymorphismHelper.ApplyMappings(options.SerializerOptions, mappings);

    public void Configure(JsonHubProtocolOptions options) 
        => JsonPolymorphismHelper.ApplyMappings(options.PayloadSerializerOptions, mappings);
}