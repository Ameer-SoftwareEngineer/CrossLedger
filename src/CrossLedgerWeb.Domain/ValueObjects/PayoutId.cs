namespace CrossLedgerWeb.Domain.ValueObjects;

public readonly record struct PayoutId(Guid Value)
{
    public static PayoutId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
