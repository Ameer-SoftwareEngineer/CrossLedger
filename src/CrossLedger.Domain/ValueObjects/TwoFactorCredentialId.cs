namespace CrossLedger.Domain.ValueObjects;

public readonly record struct TwoFactorCredentialId(Guid Value)
{
    public static TwoFactorCredentialId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
