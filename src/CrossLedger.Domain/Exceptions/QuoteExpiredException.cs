using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Domain.Exceptions;

public sealed class QuoteExpiredException : DomainException
{
    public QuoteId QuoteId { get; }
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset AttemptedAt { get; }

    public QuoteExpiredException(QuoteId quoteId, DateTimeOffset expiresAt, DateTimeOffset attemptedAt)
        : base($"Quote {quoteId} expired at {expiresAt:O}; attempted to use it at {attemptedAt:O}.")
    {
        QuoteId = quoteId;
        ExpiresAt = expiresAt;
        AttemptedAt = attemptedAt;
    }
}
