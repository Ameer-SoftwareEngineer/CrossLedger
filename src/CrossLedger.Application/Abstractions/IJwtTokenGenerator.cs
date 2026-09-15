using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Abstractions;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

public interface IJwtTokenGenerator
{
    AccessToken GenerateAccessToken(UserId userId, string email, IReadOnlyList<string> roles);
}
