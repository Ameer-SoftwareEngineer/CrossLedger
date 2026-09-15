using System.Text.Json;
using System.Text.Json.Serialization;
using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Serialization;

/// <summary>
/// Money is a record struct with a hand-written, non-positional constructor - not the
/// compiler-synthesized positional-record shape System.Text.Json specially recognizes
/// for automatic constructor binding. Left to its own devices, STJ resolves the
/// ambiguity between that constructor and a struct's implicit parameterless one by
/// picking the parameterless one, silently leaving Amount/Currency at their zero
/// defaults (they have no setters to reflect into afterwards). Found the same way as
/// <see cref="CurrencyJsonConverter"/>'s bug: replaying an idempotent transfer response
/// end to end against a running API returned amounts that were silently zeroed out.
/// </summary>
public sealed class MoneyJsonConverter : JsonConverter<Money>
{
    public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected a JSON object for Money.");

        decimal? amount = null;
        Currency? currency = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "Amount":
                    amount = reader.GetDecimal();
                    break;
                case "Currency":
                    currency = JsonSerializer.Deserialize<Currency>(ref reader, options);
                    break;
            }
        }

        if (amount is null || currency is null)
            throw new JsonException("Money requires both Amount and Currency.");

        return new Money(amount.Value, currency.Value);
    }

    public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("Amount", value.Amount);
        writer.WritePropertyName("Currency");
        JsonSerializer.Serialize(writer, value.Currency, options);
        writer.WriteEndObject();
    }
}
