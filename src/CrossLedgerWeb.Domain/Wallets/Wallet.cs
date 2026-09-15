using CrossLedgerWeb.Domain.Exceptions;
using CrossLedgerWeb.Domain.Ledger;
using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Domain.Wallets;

/// <summary>
/// A wallet never stores its own balance. <see cref="Balance"/> is derived by summing
/// the signed amount of every posted <see cref="LedgerEntry"/>, so it can never drift
/// from the entries that justify it.
/// </summary>
public sealed class Wallet
{
    private readonly List<LedgerEntry> _entries = new();

    public WalletId Id { get; }
    public UserId OwnerId { get; }
    public Currency Currency { get; }
    public WalletKind Kind { get; }

    public IReadOnlyList<LedgerEntry> Entries => _entries;

    public Money Balance => _entries.Aggregate(Money.Zero(Currency), (balance, entry) => balance + entry.SignedAmount);

    public Wallet(WalletId id, UserId ownerId, Currency currency, WalletKind kind = WalletKind.Customer)
    {
        Id = id;
        OwnerId = ownerId;
        Currency = currency;
        Kind = kind;
    }

    public LedgerEntry Debit(Money amount, TransferId transferId, DateTimeOffset postedAt) =>
        Post(LedgerDirection.Debit, amount, transferId, postedAt);

    public LedgerEntry Credit(Money amount, TransferId transferId, DateTimeOffset postedAt) =>
        Post(LedgerDirection.Credit, amount, transferId, postedAt);

    private LedgerEntry Post(LedgerDirection direction, Money amount, TransferId transferId, DateTimeOffset postedAt)
    {
        if (amount.Currency != Currency)
            throw new CurrencyMismatchException(Currency, amount.Currency);

        if (direction == LedgerDirection.Debit && Kind == WalletKind.Customer && Balance < amount)
            throw new InsufficientFundsException(Id, Balance, amount);

        var entry = new LedgerEntry(LedgerEntryId.New(), transferId, Id, direction, amount, postedAt);
        _entries.Add(entry);
        return entry;
    }
}
