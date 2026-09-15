using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Fx;

public sealed record CreateQuoteResult(QuoteId QuoteId, Currency FromCurrency, Currency ToCurrency, decimal Rate, DateTimeOffset ExpiresAt);
