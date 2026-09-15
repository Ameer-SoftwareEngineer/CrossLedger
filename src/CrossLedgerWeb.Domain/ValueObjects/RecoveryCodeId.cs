namespace CrossLedgerWeb.Domain.ValueObjects;

public readonly record struct RecoveryCodeId(Guid Value)
{
    public static RecoveryCodeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
