using CrossLedger.Application.Abstractions;
using CrossLedger.Domain.Fx;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class QuoteRepository : IQuoteRepository
{
    private readonly CrossLedgerDbContext _db;

    public QuoteRepository(CrossLedgerDbContext db)
    {
        _db = db;
    }

    public Task<Quote?> GetByIdAsync(QuoteId id, CancellationToken cancellationToken) =>
        _db.Quotes.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
}
