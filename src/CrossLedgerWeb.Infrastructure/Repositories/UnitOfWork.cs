using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Infrastructure.Persistence;

namespace CrossLedgerWeb.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CrossLedgerWebDbContext _db;

    public UnitOfWork(CrossLedgerWebDbContext db)
    {
        _db = db;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
