using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Auth;

public sealed record CredentialValidationOutcome(bool Succeeded, UserId? UserId, bool IsLockedOut)
{
    public static CredentialValidationOutcome Success(UserId userId) => new(true, userId, IsLockedOut: false);

    public static CredentialValidationOutcome Failed { get; } = new(false, null, IsLockedOut: false);

    public static CredentialValidationOutcome LockedOut { get; } = new(false, null, IsLockedOut: true);
}
