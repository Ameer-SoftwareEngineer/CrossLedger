using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Domain.Fx;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Infrastructure.Repositories;

public sealed class QuoteRepository : IQuoteRepository
{
    private readonly CrossLedgerWebDbContext _db;

    public QuoteRepository(CrossLedgerWebDbContext db)
    {
        _db = db;
    }

    public Task<Quote?> GetByIdAsync(QuoteId id, CancellationToken cancellationToken) =>
        _db.Quotes.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public void Add(Quote quote) => _db.Quotes.Add(quote);
}
