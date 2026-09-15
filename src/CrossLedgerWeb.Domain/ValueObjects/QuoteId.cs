namespace CrossLedgerWeb.Domain.ValueObjects;

public readonly record struct QuoteId(Guid Value)
{
    public static QuoteId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
