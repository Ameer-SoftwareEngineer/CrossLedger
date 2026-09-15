using System.Collections.Concurrent;
using CrossLedger.Application.Payments;
using CrossLedger.Providers.Common;

namespace CrossLedger.Providers.Simulated;

/// <summary>
/// In-process chaos and test harness (specification 5.6): serves every corridor and its
/// behaviour is entirely configuration-driven, so a test can reproduce exactly the
/// failure mode it needs - a rejected payout, an unhealthy/unavailable provider - without
/// a real sandbox. ExecuteAsync is genuinely idempotent on IdempotencyKey (a repeated
/// key returns the original recorded outcome, never re-executes), matching every real
/// provider's contract, not just this one's documentation of it.
/// </summary>
public sealed class SimulatedProvider : IPaymentProvider
{
    private readonly SimulatedProviderOptions _options;
    private readonly ConcurrentDictionary<string, PayoutResult> _executedPayouts = new();

    public SimulatedProvider(SimulatedProviderOptions? options = null)
    {
        _options = options ?? new SimulatedProviderOptions();
    }

    public ProviderCode Code => ProviderCode.Simulated;

    public bool Supports(Corridor corridor) => true;

    public Task<ProviderQuote> QuoteAsync(PayoutRequest request, CancellationToken cancellationToken)
    {
        var feeAmount = Math.Round(request.Amount.Amount * _options.FeeRatio, request.Amount.Currency.DecimalPlaces, MidpointRounding.ToEven);
        var fee = new Domain.ValueObjects.Money(feeAmount, request.Amount.Currency);

        return Task.FromResult(new ProviderQuote(Code, request.Amount, fee, _options.QuotedSettlementTime));
    }

    public Task<PayoutResult> ExecuteAsync(PayoutRequest request, CancellationToken cancellationToken)
    {
        if (_executedPayouts.TryGetValue(request.IdempotencyKey, out var existing))
            return Task.FromResult(existing);

        var result = _options.RejectAllPayouts
            ? new PayoutResult(Code, PayoutOutcome.Rejected, ProviderReference: null, FailureReason: "Simulated rejection")
            : new PayoutResult(Code, PayoutOutcome.Accepted, ProviderReference: Guid.NewGuid().ToString(), FailureReason: null);

        // GetOrAdd, not indexer assignment: two concurrent callers with the same key
        // must land on the same recorded outcome, not overwrite each other.
        return Task.FromResult(_executedPayouts.GetOrAdd(request.IdempotencyKey, result));
    }

    public bool VerifySignature(string payload, string signature, string secret) =>
        HmacSignatureVerifier.Verify(payload, signature, secret);

    public Task<ProviderHealth> CheckHealthAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new ProviderHealth(Code, _options.HealthStatus, DateTimeOffset.UtcNow));
}
