namespace CrossLedgerWeb.Shared.Fx;

public sealed record QuoteResponse(Guid QuoteId, string FromCurrency, string ToCurrency, decimal Rate, DateTimeOffset ExpiresAt);
