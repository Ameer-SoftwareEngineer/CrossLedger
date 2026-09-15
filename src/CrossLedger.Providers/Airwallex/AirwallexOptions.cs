namespace CrossLedger.Providers.Airwallex;

public sealed class AirwallexOptions
{
    public const string SectionName = "Airwallex";

    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>ISO 3166-1 alpha-2 destination countries this account is provisioned
    /// for. Empty by default; set from configuration once the sandbox account exists.</summary>
    public IReadOnlyList<string> SupportedDestinationCountries { get; set; } = [];
}
