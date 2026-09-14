namespace CrossLedger.Domain.Wallets;

public enum WalletKind
{
    /// <summary>A customer-owned wallet. Must never be debited below zero.</summary>
    Customer,

    /// <summary>An internal FX settlement / clearing account. Legitimately runs a
    /// negative balance between the two legs of a cross-currency transfer, so it is
    /// exempt from the non-negative balance check.</summary>
    SystemClearing,
}
