using CrossLedger.Domain.Exceptions;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Domain.Auth;

/// <summary>
/// The rotate-on-use rule from specification 6.4: presenting a refresh token issues a
/// new one and revokes the presented one, but presenting an ALREADY-revoked token means
/// someone else already used it first - the strongest available signal that it leaked -
/// so every other active token in the family is revoked too. The caller (an Application
/// handler) supplies every token in the family since only it can query for them; this
/// only ever operates on the in-memory set it's given.
/// </summary>
public static class RefreshTokenRotation
{
    public static RefreshToken Rotate(
        RefreshToken presented,
        IReadOnlyCollection<RefreshToken> familyTokens,
        DateTimeOffset now,
        RefreshTokenId newTokenId,
        string newTokenHash,
        TimeSpan validFor)
    {
        if (presented.IsRevoked)
        {
            foreach (var token in familyTokens.Where(t => !t.IsRevoked))
                token.Revoke(now);

            throw new RefreshTokenReuseDetectedException(presented.FamilyId);
        }

        if (presented.IsExpired(now))
            throw new RefreshTokenExpiredException(presented.Id);

        var next = new RefreshToken(newTokenId, presented.UserId, newTokenHash, presented.FamilyId, now, validFor);
        presented.Revoke(now, next.Id);

        return next;
    }
}
