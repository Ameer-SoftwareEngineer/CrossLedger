using CrossLedger.Application.Abstractions;
using CrossLedger.Infrastructure.Persistence;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CrossLedgerDbContext _db;

    public UnitOfWork(CrossLedgerDbContext db)
    {
        _db = db;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
