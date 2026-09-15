namespace CrossLedger.Application.Payments;

/// <summary>
/// Implements the weighted scoring table from specification 5.3: total cost 40%,
/// settlement speed 30%, historical reliability 20%, limit headroom 10% - with speed's
/// weight increasing for a priority transfer, matching "weighting increases when the
/// user selects a priority transfer". Standard and Priority weights were not given
/// exact numbers in the specification; the concrete split used here (cost 40/25%, speed
/// 30/45%, reliability and headroom unchanged) is this implementation's own choice,
/// documented so it can be tuned later without hunting through the scoring logic.
/// </summary>
public sealed class ProviderQuoteScorer : IProviderQuoteScorer
{
    private static readonly TimeSpan SlowestReasonableSettlement = TimeSpan.FromHours(24);

    public decimal Score(ProviderQuote quote, ProviderStats stats, RoutingPreference preference)
    {
        var costScore = ScoreCost(quote);
        var speedScore = ScoreSpeed(quote.EstimatedSettlementTime);
        var reliabilityScore = Math.Clamp(stats.RollingSuccessRate, 0m, 1m);
        var headroomScore = Math.Clamp(stats.RemainingDailyHeadroomRatio, 0m, 1m);

        var (costWeight, speedWeight) = preference == RoutingPreference.Priority
            ? (costWeight: 0.25m, speedWeight: 0.45m)
            : (costWeight: 0.40m, speedWeight: 0.30m);

        return (costScore * costWeight)
            + (speedScore * speedWeight)
            + (reliabilityScore * 0.20m)
            + (headroomScore * 0.10m);
    }

    private static decimal ScoreCost(ProviderQuote quote)
    {
        if (quote.Amount.Amount <= 0)
            return 0m;

        var feeRatio = quote.Fee.Amount / quote.Amount.Amount;
        return Math.Clamp(1m - feeRatio, 0m, 1m);
    }

    private static decimal ScoreSpeed(TimeSpan estimatedSettlementTime)
    {
        if (estimatedSettlementTime >= SlowestReasonableSettlement)
            return 0m;

        var ratio = 1m - ((decimal)estimatedSettlementTime.TotalMinutes / (decimal)SlowestReasonableSettlement.TotalMinutes);
        return Math.Clamp(ratio, 0m, 1m);
    }
}
