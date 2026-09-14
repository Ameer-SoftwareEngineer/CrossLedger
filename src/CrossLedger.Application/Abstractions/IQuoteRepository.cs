using CrossLedger.Domain.Fx;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

public interface IQuoteRepository
{
    Task<Quote?> GetByIdAsync(QuoteId id, CancellationToken cancellationToken);
}
