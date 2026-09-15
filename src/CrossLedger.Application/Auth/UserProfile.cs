using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Auth;

public sealed record UserProfile(UserId UserId, string Email, IReadOnlyList<string> Roles);
