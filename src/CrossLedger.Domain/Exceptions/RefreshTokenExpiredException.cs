using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Domain.Exceptions;

public sealed class RefreshTokenExpiredException : DomainException
{
    public RefreshTokenId RefreshTokenId { get; }

    public RefreshTokenExpiredException(RefreshTokenId refreshTokenId)
        : base($"Refresh token {refreshTokenId} has expired.")
    {
        RefreshTokenId = refreshTokenId;
    }
}
