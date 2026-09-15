using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Domain.Exceptions;

public sealed class RefreshTokenExpiredException : DomainException
{
    public RefreshTokenId RefreshTokenId { get; }

    public RefreshTokenExpiredException(RefreshTokenId refreshTokenId)
        : base($"Refresh token {refreshTokenId} has expired.")
    {
        RefreshTokenId = refreshTokenId;
    }
}
