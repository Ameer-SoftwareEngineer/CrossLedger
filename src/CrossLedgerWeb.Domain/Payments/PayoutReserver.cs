using CrossLedgerWeb.Domain.Ledger;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Domain.Wallets;

namespace CrossLedgerWeb.Domain.Payments;

/// <summary>
/// Reserves funds for an outbound payout: debits the customer's wallet, credits a
/// PayoutReserve wallet in the same currency, and constructs the <see cref="Payout"/>
/// that tracks the external attempt against that reservation (specification 5.5's
/// RESERVED state - "funds held, ledger entries written"). Two legs, not TransferPoster's
/// four: a payout has no second, internal leg to post - the money is leaving the platform
/// entirely, not moving between two of the platform's own wallets. Whatever currency
/// conversion the corridor needs happens on the provider's side, not in this ledger.
/// </summary>
public static class PayoutReserver
{
    public static (Payout Payout, IReadOnlyList<LedgerEntry> Entries) Reserve(
        PayoutId payoutId,
        TransferId transferId,
        Wallet sourceWallet,
        Wallet payoutReserveWallet,
        Money amount,
        DateTimeOffset postedAt)
    {
        var entries = new[]
        {
            sourceWallet.Debit(amount, transferId, postedAt),
            payoutReserveWallet.Credit(amount, transferId, postedAt),
        };

        LedgerInvariants.AssertZeroSumPerCurrency(entries);

        var payout = new Payout(payoutId, transferId, sourceWallet.Id, amount);
        payout.Reserve();

        return (payout, entries);
    }

    /// <summary>Posts the compensating entries specification 5.5 describes for "any state
    /// -> REVERSED" - reserved funds move back out of the reserve wallet and back onto the
    /// customer's wallet - and only then flips the Payout's own state. <see cref="Payout.Reverse"/>
    /// by itself is just the state flag; this is what actually moves the money back.</summary>
    public static IReadOnlyList<LedgerEntry> ReverseReservation(
        Payout payout,
        Wallet sourceWallet,
        Wallet payoutReserveWallet,
        DateTimeOffset postedAt)
    {
        var entries = new[]
        {
            payoutReserveWallet.Debit(payout.Amount, payout.TransferId, postedAt),
            sourceWallet.Credit(payout.Amount, payout.TransferId, postedAt),
        };

        LedgerInvariants.AssertZeroSumPerCurrency(entries);

        payout.Reverse();

        return entries;
    }
}
