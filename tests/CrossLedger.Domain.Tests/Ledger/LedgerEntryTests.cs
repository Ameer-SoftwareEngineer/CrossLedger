using CrossLedger.Domain.Ledger;
using CrossLedger.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedger.Domain.Tests.Ledger;

public class LedgerEntryTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    public void SignedAmount_is_negative_for_a_debit()
    {
        var entry = new LedgerEntry(
            LedgerEntryId.New(), TransferId.New(), WalletId.New(),
            LedgerDirection.Debit, new Money(100m, Usd), Now);

        entry.SignedAmount.Amount.Should().Be(-100m);
    }

    [Fact]
    public void SignedAmount_is_positive_for_a_credit()
    {
        var entry = new LedgerEntry(
            LedgerEntryId.New(), TransferId.New(), WalletId.New(),
            LedgerDirection.Credit, new Money(100m, Usd), Now);

        entry.SignedAmount.Amount.Should().Be(100m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_rejects_a_non_positive_amount(decimal amount)
    {
        var act = () => new LedgerEntry(
            LedgerEntryId.New(), TransferId.New(), WalletId.New(),
            LedgerDirection.Debit, new Money(amount, Usd), Now);

        act.Should().Throw<ArgumentException>();
    }
}
