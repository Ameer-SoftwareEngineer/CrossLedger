using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Domain.Auth;

/// <summary>
/// One issued refresh token. TokenHash, never the raw token, is what's persisted -
/// refresh tokens are long-lived bearer credentials, so a leaked database must not hand
/// out usable sessions. FamilyId links every token descended from a single login: all of
/// them get revoked together the moment <see cref="RefreshTokenRotation"/> detects reuse
/// (specification 6.4).
/// </summary>
public sealed class RefreshToken
{
    public RefreshTokenId Id { get; }
    public UserId UserId { get; }
    public string TokenHash { get; }
    public Guid FamilyId { get; }
    public DateTimeOffset IssuedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public RefreshTokenId? ReplacedByTokenId { get; private set; }

    public RefreshToken(
        RefreshTokenId id,
        UserId userId,
        string tokenHash,
        Guid familyId,
        DateTimeOffset issuedAt,
        TimeSpan validFor)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("A refresh token must have a token hash.", nameof(tokenHash));

        if (validFor <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(validFor), validFor, "A refresh token must be valid for a positive duration.");

        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        IssuedAt = issuedAt;
        ExpiresAt = issuedAt + validFor;
    }

    /// <summary>Rehydration constructor for EF Core materialization: the public
    /// constructor takes validFor (a TimeSpan) with no matching stored property -
    /// ExpiresAt is stored directly - so EF Core cannot bind it, the same issue
    /// Quote's rehydration constructor exists for. RevokedAt and ReplacedByTokenId are
    /// deliberately left out here too - both already have a private setter, so EF sets
    /// them directly after construction instead. Skips validation intentionally: a
    /// persisted row was already valid when it was created.</summary>
    private RefreshToken(
        RefreshTokenId id,
        UserId userId,
        string tokenHash,
        Guid familyId,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsExpired(DateTimeOffset asOf) => asOf >= ExpiresAt;

    public bool IsActive(DateTimeOffset asOf) => !IsRevoked && !IsExpired(asOf);

    /// <summary>Idempotent on purpose: revoking an already-revoked token (e.g. every
    /// surviving member of a family during reuse-triggered revocation) is a no-op, not
    /// an error - only the first revocation's timestamp and reason are kept.</summary>
    public void Revoke(DateTimeOffset revokedAt, RefreshTokenId? replacedByTokenId = null)
    {
        if (IsRevoked)
            return;

        RevokedAt = revokedAt;
        ReplacedByTokenId = replacedByTokenId;
    }
}
