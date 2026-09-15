namespace CrossLedgerWeb.Application.Payments;

/// <summary>The two scoring signals that need historical data rather than a live
/// provider call: rolling 24h success rate and remaining daily volume allowance
/// (specification 5.3). Both are ratios in [0, 1].</summary>
public sealed record ProviderStats(decimal RollingSuccessRate, decimal RemainingDailyHeadroomRatio)
{
    public static ProviderStats Unknown { get; } = new(RollingSuccessRate: 1m, RemainingDailyHeadroomRatio: 1m);
}
