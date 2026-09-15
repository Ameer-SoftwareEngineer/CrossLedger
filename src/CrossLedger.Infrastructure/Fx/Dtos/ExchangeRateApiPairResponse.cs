using System.Text.Json.Serialization;

namespace CrossLedger.Infrastructure.Fx.Dtos;

/// <summary>Shape of the ExchangeRate-API pair-conversion endpoint response
/// (v6/{key}/pair/{from}/{to}). Never used outside <see cref="ExchangeRateApiProvider"/> -
/// callers only ever see <see cref="CrossLedger.Application.Fx.ExchangeRateReading"/>.</summary>
internal sealed class ExchangeRateApiPairResponse
{
    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("error-type")]
    public string? ErrorType { get; set; }

    [JsonPropertyName("time_last_update_unix")]
    public long TimeLastUpdateUnix { get; set; }

    [JsonPropertyName("base_code")]
    public string? BaseCode { get; set; }

    [JsonPropertyName("target_code")]
    public string? TargetCode { get; set; }

    [JsonPropertyName("conversion_rate")]
    public decimal? ConversionRate { get; set; }
}
