using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Infrastructure;

/// <summary>Thrown when no PayoutReserve wallet exists for a currency the platform is
/// trying to pay out in. An operator error, never a client-facing 404 - it should page
/// operations, not the caller (mirrors SettlementWalletNotConfiguredException).</summary>
public sealed class PayoutReserveWalletNotConfiguredException : Exception
{
    public Currency Currency { get; }

    public PayoutReserveWalletNotConfiguredException(Currency currency)
        : base($"No payout reserve wallet is configured for {currency.Code}.")
    {
        Currency = currency;
    }
}
