using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Domain.Exceptions;

public sealed class CurrencyMismatchException : DomainException
{
    public Currency Left { get; }
    public Currency Right { get; }

    public CurrencyMismatchException(Currency left, Currency right)
        : base($"Cannot combine amounts in different currencies: {left.Code} and {right.Code}.")
    {
        Left = left;
        Right = right;
    }
}
