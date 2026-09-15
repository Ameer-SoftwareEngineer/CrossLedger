namespace CrossLedger.Domain.ValueObjects;

public readonly record struct UsedTotpCodeId(Guid Value)
{
    public static UsedTotpCodeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
