using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Exceptions;
using CrossLedger.Domain.Auth;
using CrossLedger.Domain.Exceptions;
using CrossLedger.Domain.ValueObjects;
using MediatR;

namespace CrossLedger.Application.Auth;

/// <summary>Rotates a refresh token per specification 6.4. RefreshTokenRotation (Domain)
/// throws RefreshTokenReuseDetectedException if the presented token was already used -
/// that propagates from here unchanged, since a caller presenting a dead token is
/// exactly the signal that warrants rejecting the request outright.</summary>
public sealed class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, LoginResult>
{
    private readonly IIdentityService _identity;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RefreshAccessTokenCommandHandler(
        IIdentityService identity,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _identity = identity;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<LoginResult> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var presentedHash = RefreshTokenGenerator.Hash(request.RefreshToken);
        var presented = await _refreshTokens.GetByTokenHashAsync(presentedHash, cancellationToken)
            ?? throw new InvalidRefreshTokenException();

        var family = await _refreshTokens.GetFamilyAsync(presented.FamilyId, cancellationToken);

        var now = _clock.UtcNow;
        var rawNextToken = RefreshTokenGenerator.GenerateRawToken();

        RefreshToken next;
        try
        {
            next = RefreshTokenRotation.Rotate(
                presented, family, now, RefreshTokenId.New(), RefreshTokenGenerator.Hash(rawNextToken), LoginCommandHandler.RefreshTokenValidity);
        }
        catch (RefreshTokenReuseDetectedException)
        {
            // Rotate() already revoked every sibling in-memory before throwing. The
            // pipeline's UnitOfWorkBehavior only commits after a *successful* next() -
            // it never runs when this handler throws - so without an explicit save here,
            // the strongest signal that a token leaked would be computed and then
            // silently discarded instead of reaching the database. Found by actually
            // running the reuse scenario against a real database, not by the (in-memory,
            // mock-only) unit tests, which never exercised this commit timing at all.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }

        _refreshTokens.Add(next);

        var profile = await _identity.GetProfileAsync(presented.UserId, cancellationToken)
            ?? throw new InvalidRefreshTokenException();

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(presented.UserId, profile.Email, profile.Roles);

        return new LoginResult(accessToken.Value, accessToken.ExpiresAt, rawNextToken, next.ExpiresAt);
    }
}
