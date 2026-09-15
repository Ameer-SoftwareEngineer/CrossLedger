using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Domain.Auth;

/// <summary>
/// A user's enrolled TOTP secret (specification 6.1). Enrolment integrity is enforced by
/// the Application layer, not here: this entity is only ever constructed after the user
/// has already proven they can generate a valid code from the secret, so - unlike
/// Wallet or Payout - there is no separate "pending, unverified" state to represent;
/// IsEnabled starts true and only ever moves to false via Disable().
/// </summary>
public sealed class TwoFactorCredential
{
    public TwoFactorCredentialId Id { get; }
    public UserId UserId { get; }

    /// <summary>The raw TOTP secret. Encryption at rest (specification 6.4) is a
    /// persistence concern applied by Infrastructure, not something this type needs to
    /// know about.</summary>
    public string Secret { get; }

    public bool IsEnabled { get; private set; }
    public DateTimeOffset EnrolledAt { get; }

    public TwoFactorCredential(TwoFactorCredentialId id, UserId userId, string secret, DateTimeOffset enrolledAt)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("A TOTP secret is required.", nameof(secret));

        Id = id;
        UserId = userId;
        Secret = secret;
        EnrolledAt = enrolledAt;
        IsEnabled = true;
    }

    public void Disable() => IsEnabled = false;
}
