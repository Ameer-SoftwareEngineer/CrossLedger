using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Exceptions;

public sealed class QuoteNotFoundException : Exception
{
    public QuoteId QuoteId { get; }

    public QuoteNotFoundException(QuoteId quoteId)
        : base($"Quote {quoteId} was not found.")
    {
        QuoteId = quoteId;
    }
}
