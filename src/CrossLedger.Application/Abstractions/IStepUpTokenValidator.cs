using CrossLedger.Application.Auth;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

public sealed record StepUpTokenValidationResult(bool IsValid, UserId? UserId, StepUpOperation? Operation)
{
    public static StepUpTokenValidationResult Invalid { get; } = new(false, null, null);

    public static StepUpTokenValidationResult Valid(UserId userId, StepUpOperation operation) =>
        new(true, userId, operation);
}

/// <summary>The API layer's [RequireStepUp] filter depends on this, not on a JWT
/// library directly - the same separation IJwtTokenGenerator gives the write side.</summary>
public interface IStepUpTokenValidator
{
    StepUpTokenValidationResult Validate(string token, StepUpOperation expectedOperation);
}
