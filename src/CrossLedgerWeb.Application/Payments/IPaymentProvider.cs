namespace CrossLedgerWeb.Application.Payments;

/// <summary>
/// Every payout rail implements exactly this interface (specification 5.2). Adding a
/// fifth provider requires no change to any existing class - the Open/Closed Principle
/// in the routing engine's own terms.
/// </summary>
public interface IPaymentProvider
{
    ProviderCode Code { get; }

    /// <summary>Can this provider serve this corridor at all? A hard filter in the
    /// routing engine, evaluated before any network call.</summary>
    bool Supports(Corridor corridor);

    Task<ProviderQuote> QuoteAsync(PayoutRequest request, CancellationToken cancellationToken);

    /// <summary>Must be idempotent on <see cref="PayoutRequest.IdempotencyKey"/> -
    /// providers do not guarantee they'll only see a request once.</summary>
    Task<PayoutResult> ExecuteAsync(PayoutRequest request, CancellationToken cancellationToken);

    /// <summary>Verifies an inbound webhook actually came from this provider.</summary>
    bool VerifySignature(string payload, string signature, string secret);

    Task<ProviderHealth> CheckHealthAsync(CancellationToken cancellationToken);
}
