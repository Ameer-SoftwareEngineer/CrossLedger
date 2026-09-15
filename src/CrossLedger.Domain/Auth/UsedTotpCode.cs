using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Domain.Auth;

/// <summary>
/// TOTP replay protection (specification 6.3): "a consumed code is recorded and cannot
/// be presented twice within its validity window." Recording the raw code (not a hash)
/// is deliberate - a 6-digit TOTP code has only a million possible values and is only
/// ever meaningful for a ~90 second window tied to one user, so it carries none of a
/// password's confidentiality requirement; what matters is a fast, exact lookup to
/// reject a replay before it reaches the (comparatively expensive) TOTP verification.
/// </summary>
public sealed class UsedTotpCode
{
    public UsedTotpCodeId Id { get; }
    public UserId UserId { get; }
    public string Code { get; }
    public DateTimeOffset UsedAt { get; }
    public DateTimeOffset ExpiresAt { get; }

    public UsedTotpCode(UsedTotpCodeId id, UserId userId, string code, DateTimeOffset usedAt, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("A TOTP code is required.", nameof(code));

        Id = id;
        UserId = userId;
        Code = code;
        UsedAt = usedAt;
        ExpiresAt = expiresAt;
    }

    public bool IsActive(DateTimeOffset asOf) => asOf < ExpiresAt;
}
