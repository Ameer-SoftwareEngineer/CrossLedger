using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Infrastructure;

/// <summary>Thrown when no SystemClearing wallet exists for a currency the platform is
/// trying to transact in. This is an operator error (a currency was enabled for FX
/// quotes without provisioning its settlement account), never a client-facing 404 - it
/// should page operations, not the caller.</summary>
public sealed class SettlementWalletNotConfiguredException : Exception
{
    public Currency Currency { get; }

    public SettlementWalletNotConfiguredException(Currency currency)
        : base($"No FX settlement wallet is configured for {currency.Code}.")
    {
        Currency = currency;
    }
}
