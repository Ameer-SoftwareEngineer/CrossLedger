using CrossLedger.Application.Auth;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

public interface IJwtTokenGenerator
{
    AccessToken GenerateAccessToken(UserId userId, string email, IReadOnlyList<string> roles);

    /// <summary>Short-lived (5 minutes, specification 6.1), scoped to exactly one
    /// operation class - a step-up token minted for a transfer can't be reused to
    /// authorize disabling two-factor auth.</summary>
    AccessToken GenerateStepUpToken(UserId userId, StepUpOperation operation);
}
