using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ukinee.Infrastructure.Json.Services;

public class JsonStringFlagsEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum && typeToConvert.IsDefined(typeof(FlagsAttribute), false);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(JsonStringFlagsEnumConverter<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class JsonStringFlagsEnumConverter<T> : JsonConverter<T>
where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Expected JSON Array for Flags enum {typeof(T).Name}");

        ulong mask = 0;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var token = reader.GetString();

            if (Enum.TryParse<T>(token, out var result))
            {
                mask |= Convert.ToUInt64(result);
            }
        }

        return (T)Enum.ToObject(typeof(T), mask);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var flag in Enum.GetValues<T>())
        {
            var flagValue = Convert.ToUInt64(flag);

            if (flagValue == 0 || (flagValue & flagValue - 1) != 0)
                continue;

            if (value.HasFlag(flag))
            {
                writer.WriteStringValue(flag.ToString());
            }
        }

        writer.WriteEndArray();
    }
}
