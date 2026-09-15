namespace CrossLedgerWeb.Shared.Payments;

/// <summary>Wire contract for POST /api/v1/payouts. The idempotency key travels as the
/// Idempotency-Key header (specification 2.3), not a body field - same convention as
/// CreateTransferRequest. Preference is "Standard" or "Priority" on the wire.</summary>
public sealed record CreatePayoutRequest(
    Guid SourceWalletId,
    string TargetCurrency,
    string DestinationCountry,
    decimal Amount,
    string Preference);

public sealed record PayoutResponse(
    Guid PayoutId,
    Guid TransferId,
    string State,
    string? ProviderCode,
    string? ProviderReference);
