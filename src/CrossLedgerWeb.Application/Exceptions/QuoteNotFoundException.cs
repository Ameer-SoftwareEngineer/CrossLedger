using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Exceptions;

public sealed class QuoteNotFoundException : Exception
{
    public QuoteId QuoteId { get; }

    public QuoteNotFoundException(QuoteId quoteId)
        : base($"Quote {quoteId} was not found.")
    {
        QuoteId = quoteId;
    }
}
