using System.Text.Json;

namespace Ukinee.Infrastructure.Json.Contracts;

public interface IJsonOptionsProvider
{
    JsonSerializerOptions Options { get; }
}

// ReSharper disable once UnusedTypeParameter
public interface IJsonOptionsProvider<TConsumer> : IJsonOptionsProvider;