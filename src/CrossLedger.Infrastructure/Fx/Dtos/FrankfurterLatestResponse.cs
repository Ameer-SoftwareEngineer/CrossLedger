using System.Text.Json.Serialization;

namespace CrossLedger.Infrastructure.Fx.Dtos;

/// <summary>Shape of the Frankfurter `/latest` endpoint response (ECB-sourced, no API
/// key). Never used outside <see cref="FrankfurterProvider"/>.</summary>
internal sealed class FrankfurterLatestResponse
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateOnly Date { get; set; }

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
