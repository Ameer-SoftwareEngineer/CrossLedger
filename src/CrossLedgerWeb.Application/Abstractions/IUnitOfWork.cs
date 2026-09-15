namespace CrossLedgerWeb.Application.Abstractions;

/// <summary>Commits everything a handler changed as one atomic unit — e.g. the ledger
/// entries appended to several wallets during a single transfer.</summary>
public interface IUnitOfWork
{
    /// <summary>Throws Exceptions.ConcurrencyConflictException, never the underlying
    /// ORM's own concurrency exception type, if a tracked aggregate's RowVersion no
    /// longer matches what's in the database (specification 2.4).</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>Discards every tracked entity so the next read within this request is
    /// forced back to the database - what a concurrency-conflict retry needs before
    /// re-running the handler, or its "fresh" reload would just return the same stale
    /// in-memory instance EF already has cached.</summary>
    void ClearTrackedChanges();
}
