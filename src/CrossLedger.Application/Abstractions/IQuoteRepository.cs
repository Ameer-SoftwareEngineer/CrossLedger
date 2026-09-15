using CrossLedger.Domain.Fx;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

public interface IQuoteRepository
{
    Task<Quote?> GetByIdAsync(QuoteId id, CancellationToken cancellationToken);

    /// <summary>Stages a new quote for insertion - committed by UnitOfWorkBehavior's
    /// SaveChanges at the end of the pipeline, not immediately.</summary>
    void Add(Quote quote);
}
