namespace CrossLedger.Infrastructure.Fx;

public sealed class ExchangeRateApiOptions
{
    public const string SectionName = "ExchangeRateApi";

    public string ApiKey { get; set; } = string.Empty;
}
