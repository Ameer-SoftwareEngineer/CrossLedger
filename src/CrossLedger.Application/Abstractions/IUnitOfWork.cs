namespace CrossLedger.Application.Abstractions;

/// <summary>Commits everything a handler changed as one atomic unit — e.g. the ledger
/// entries appended to several wallets during a single transfer.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
