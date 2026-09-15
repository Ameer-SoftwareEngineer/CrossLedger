using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Domain.Wallets;

namespace CrossLedgerWeb.Application.Abstractions;

/// <summary>Resolves the platform's PayoutReserve wallet for a currency - where funds
/// land while an outbound payout is in flight to an external provider (specification
/// 5.5's RESERVED state). Mirrors <see cref="IFxSettlementWalletResolver"/>'s shape
/// deliberately; the two are kept as separate resolvers because they resolve different
/// WalletKinds for different reasons, not because the lookup itself differs.</summary>
public interface IPayoutReserveWalletResolver
{
    Task<Wallet> GetPayoutReserveWalletAsync(Currency currency, CancellationToken cancellationToken);
}
