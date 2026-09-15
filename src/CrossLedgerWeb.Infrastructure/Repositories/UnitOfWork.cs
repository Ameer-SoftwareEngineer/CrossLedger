using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Exceptions;
using CrossLedgerWeb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CrossLedgerWebDbContext _db;

    public UnitOfWork(CrossLedgerWebDbContext db)
    {
        _db = db;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Translated rather than left to propagate - Application has no reference to
            // EF Core and cannot catch this type itself (specification 2.4's retry logic
            // lives in ConcurrencyRetryBehavior, in Application).
            throw new ConcurrencyConflictException(ex);
        }
    }

    public void ClearTrackedChanges() => _db.ChangeTracker.Clear();
}
