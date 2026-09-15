using CrossLedgerWeb.Application.Payments;
using CrossLedgerWeb.Domain.Payments;

namespace CrossLedgerWeb.Infrastructure.Repositories;

/// <summary>
/// Returns full marks for every provider - a documented placeholder, not a finished
/// feature. Computing genuine rolling 24h success rate and remaining daily headroom
/// (specification 5.3) needs a persisted history of past payout outcomes, which is a
/// separate feature (recording real ExecuteAsync/webhook results over time) from the
/// routing engine itself. Until that exists, every provider scores as if it were
/// perfectly reliable with full capacity, so routing decisions are driven entirely by
/// the two signals this pass can actually measure: cost and speed.
/// </summary>
public sealed class DefaultProviderStatsProvider : IProviderStatsProvider
{
    public Task<ProviderStats> GetStatsAsync(ProviderCode providerCode, CancellationToken cancellationToken) =>
        Task.FromResult(ProviderStats.Unknown);
}
