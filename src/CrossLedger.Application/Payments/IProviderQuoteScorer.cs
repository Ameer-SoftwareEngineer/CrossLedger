namespace CrossLedger.Application.Payments;

public interface IProviderQuoteScorer
{
    decimal Score(ProviderQuote quote, ProviderStats stats, RoutingPreference preference);
}
