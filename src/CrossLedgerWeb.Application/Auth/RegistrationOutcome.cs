using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Auth;

public sealed record RegistrationOutcome(bool Succeeded, UserId? UserId, IReadOnlyList<string> Errors)
{
    public static RegistrationOutcome Success(UserId userId) => new(true, userId, []);

    public static RegistrationOutcome Failure(IReadOnlyList<string> errors) => new(false, null, errors);
}
