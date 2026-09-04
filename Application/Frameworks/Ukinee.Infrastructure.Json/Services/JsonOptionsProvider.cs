using System.Text.Json;
using Ukinee.Infrastructure.Json.Contracts;
using Ukinee.Infrastructure.Json.Models;
using Ukinee.Infrastructure.Json.Utils;

namespace Ukinee.Infrastructure.Json.Services;

public class JsonOptionsProvider<TConsumer> : IJsonOptionsProvider<TConsumer>
{
    public JsonSerializerOptions Options { get; }

    public JsonOptionsProvider(
        IEnumerable<PolymorphicMapping> mappings,
        Action<JsonSerializerOptions>? configureOptions = null
    )
    {
        Options = new JsonSerializerOptions();

        configureOptions?.Invoke(Options);

        JsonPolymorphismHelper.ApplyMappings(Options, mappings);
    }
}
