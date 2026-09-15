using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Fx;

public sealed record CreateQuoteResult(QuoteId QuoteId, Currency FromCurrency, Currency ToCurrency, decimal Rate, DateTimeOffset ExpiresAt);
