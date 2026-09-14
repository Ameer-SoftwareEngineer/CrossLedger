using CrossLedger.Domain.ValueObjects;
using CrossLedger.Domain.Wallets;

namespace CrossLedger.Application.Abstractions;

/// <summary>Resolves the platform's internal clearing wallet for a currency — the
/// "FX Settlement (USD)" / "FX Settlement (PKR)" accounts every cross-currency transfer
/// posts through (specification section 2.1). Callers only ever need the wallet for a
/// currency, never how it's configured or looked up.</summary>
public interface IFxSettlementWalletResolver
{
    Task<Wallet> GetSettlementWalletAsync(Currency currency, CancellationToken cancellationToken);
}
