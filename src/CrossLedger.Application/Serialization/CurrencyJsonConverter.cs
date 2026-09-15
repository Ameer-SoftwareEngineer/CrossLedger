using System.Text.Json;
using System.Text.Json.Serialization;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Serialization;

/// <summary>
/// Currency's constructor is deliberately private (only Currency.From validates and
/// constructs one) - so System.Text.Json has no way to rebuild it via reflection and
/// silently produces a default(Currency) (Code = null) instead of throwing. Discovered
/// by actually replaying an idempotent transfer response end to end: the amounts came
/// back correctly but every currency code came back null. This converter routes
/// deserialization through Currency.From, and serializes as just the ISO code.
/// </summary>
public sealed class CurrencyJsonConverter : JsonConverter<Currency>
{
    public override Currency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var code = reader.GetString();

        if (string.IsNullOrWhiteSpace(code))
            throw new JsonException("Expected a non-empty currency code.");

        return Currency.From(code);
    }

    public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Code);
}
