using CrossLedger.Domain.ValueObjects;
using CrossLedger.Domain.Wallets;

namespace CrossLedger.Application.Abstractions;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(WalletId id, CancellationToken cancellationToken);
}
