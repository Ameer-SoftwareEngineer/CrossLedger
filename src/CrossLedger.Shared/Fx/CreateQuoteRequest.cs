namespace CrossLedger.Shared.Fx;

/// <summary>Wire contract for POST /api/v1/quotes (specification 2.2's QUOTE LIFECYCLE).</summary>
public sealed record CreateQuoteRequest(string FromCurrency, string ToCurrency, decimal Amount);
