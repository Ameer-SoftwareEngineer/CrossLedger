using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Domain.Ledger;

/// <summary>
/// One immutable posting against a wallet. Ledger entries are append-only: a correction
/// is made by posting a new reversing entry, never by mutating or deleting this one — so
/// this type exposes no setters and no way to change an entry once constructed.
/// </summary>
public sealed class LedgerEntry
{
    public LedgerEntryId Id { get; }
    public TransferId TransferId { get; }
    public WalletId WalletId { get; }
    public LedgerDirection Direction { get; }

    /// <summary>Always a positive magnitude — <see cref="Direction"/> carries the sign.</summary>
    public Money Amount { get; }

    public DateTimeOffset PostedAt { get; }

    public LedgerEntry(
        LedgerEntryId id,
        TransferId transferId,
        WalletId walletId,
        LedgerDirection direction,
        Money amount,
        DateTimeOffset postedAt)
    {
        if (amount.Amount <= 0)
            throw new ArgumentException("A ledger entry amount must be a positive magnitude; direction encodes the sign.", nameof(amount));

        Id = id;
        TransferId = transferId;
        WalletId = walletId;
        Direction = direction;
        Amount = amount;
        PostedAt = postedAt;
    }

    /// <summary>Rehydration constructor for EF Core materialization: Amount maps as a
    /// complex property, which EF Core cannot bind through a constructor parameter, so
    /// this constructor omits it and EF sets it directly via the backing field afterward.
    /// The validation in the public constructor is intentionally skipped here - a row
    /// that made it into storage was already valid when it was created.</summary>
    private LedgerEntry(LedgerEntryId id, TransferId transferId, WalletId walletId, LedgerDirection direction, DateTimeOffset postedAt)
    {
        Id = id;
        TransferId = transferId;
        WalletId = walletId;
        Direction = direction;
        PostedAt = postedAt;
        Amount = default;
    }

    /// <summary>The signed contribution of this entry to its wallet's balance: negative for a debit, positive for a credit.</summary>
    public Money SignedAmount => Direction == LedgerDirection.Debit ? -Amount : Amount;
}
