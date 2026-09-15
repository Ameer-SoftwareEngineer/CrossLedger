namespace CrossLedger.Application.Payments;

public sealed record ScoredProviderQuote(ProviderQuote Quote, decimal Score);

public sealed record RoutingDecision(ScoredProviderQuote Primary, IReadOnlyList<ScoredProviderQuote> Fallbacks);
