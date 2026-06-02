using System.Text.Json;
using System.Text.Json.Serialization;

namespace FacturacionMedica.Infrastructure.Converters;

/// <summary>
/// Handles JSON numbers stored as floating-point (e.g. 1569.0) deserializing into int fields.
/// </summary>
public class IntFromDecimalConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int intValue))
                return intValue;

            if (reader.TryGetDouble(out double doubleValue))
                return (int)doubleValue;
        }

        throw new JsonException($"Cannot convert token '{reader.TokenType}' to int.");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
