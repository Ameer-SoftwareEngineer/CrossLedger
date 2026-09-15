using CrossLedger.Application.Payments;

namespace CrossLedger.Providers.Simulated;

/// <summary>Knobs for the chaos scenarios in specification 5.6 - each test configures
/// the instance it needs rather than the provider guessing what failure to inject.</summary>
public sealed class SimulatedProviderOptions
{
    public ProviderHealthStatus HealthStatus { get; set; } = ProviderHealthStatus.Healthy;
    public bool RejectAllPayouts { get; set; }
    public TimeSpan QuotedSettlementTime { get; set; } = TimeSpan.FromMinutes(5);
    public decimal FeeRatio { get; set; } = 0.01m;
    public string WebhookSecret { get; set; } = "simulated-webhook-secret";
}
