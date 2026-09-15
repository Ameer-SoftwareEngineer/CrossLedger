namespace CrossLedger.Domain.Exceptions;

/// <summary>A refresh token that was already consumed (rotated away or revoked) was
/// presented again - the strongest signal that a refresh token has leaked. Every other
/// active token in the same family is revoked when this is thrown (specification 6.4:
/// "reuse of a consumed refresh token revokes the entire family, which detects theft").</summary>
public sealed class RefreshTokenReuseDetectedException : DomainException
{
    public Guid FamilyId { get; }

    public RefreshTokenReuseDetectedException(Guid familyId)
        : base($"Refresh token reuse detected for family {familyId}; the entire family has been revoked.")
    {
        FamilyId = familyId;
    }
}
