using CrossLedger.Domain.ValueObjects;
using CrossLedger.Domain.Wallets;

namespace CrossLedger.Application.Abstractions;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(WalletId id, CancellationToken cancellationToken);

    /// <summary>Stages a new wallet for insertion - committed by UnitOfWorkBehavior's
    /// SaveChanges at the end of the pipeline, not immediately.</summary>
    void Add(Wallet wallet);
}
