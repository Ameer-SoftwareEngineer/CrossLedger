namespace CrossLedgerWeb.Providers.Rapyd;

public sealed class RapydOptions
{
    public const string SectionName = "Rapyd";

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public IReadOnlyList<string> SupportedDestinationCountries { get; set; } = [];
}
