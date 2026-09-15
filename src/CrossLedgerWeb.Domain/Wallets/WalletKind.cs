namespace CrossLedgerWeb.Domain.Wallets;

public enum WalletKind
{
    /// <summary>A customer-owned wallet. Must never be debited below zero.</summary>
    Customer,

    /// <summary>An internal FX settlement / clearing account. Legitimately runs a
    /// negative balance between the two legs of a cross-currency transfer, so it is
    /// exempt from the non-negative balance check.</summary>
    SystemClearing,

    /// <summary>Holds funds debited from a customer wallet for a payout that is in
    /// flight to an external provider (specification 5.5's RESERVED state: "funds held,
    /// ledger entries written"). Distinct from <see cref="SystemClearing"/> - that account
    /// is for the two legs of an internal, same-platform FX transfer; this one is for
    /// money that is actually leaving the platform. Exempt from the non-negative check for
    /// the same reason SystemClearing is.</summary>
    PayoutReserve,
}
