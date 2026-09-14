namespace CrossLedger.Domain.ValueObjects;

public readonly record struct LedgerEntryId(Guid Value)
{
    public static LedgerEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
