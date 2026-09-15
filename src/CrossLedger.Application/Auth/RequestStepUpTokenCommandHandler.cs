using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Exceptions;
using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using MediatR;

namespace CrossLedger.Application.Auth;

/// <summary>Issues a step-up token (specification 6.1) only after a fresh, unused TOTP
/// code verifies against the caller's enrolled secret. Order matters: the replay check
/// runs before VerifyCode so a replayed code is rejected without a second (comparatively
/// expensive) TOTP computation, and the code is only recorded as used once it has
/// actually verified - a wrong code must remain retryable.</summary>
public sealed class RequestStepUpTokenCommandHandler : IRequestHandler<RequestStepUpTokenCommand, StepUpTokenResult>
{
    // Covers a standard 30-second TOTP step plus the +/-1 step skew tolerance
    // ITotpProvider implementations typically allow, with margin.
    private static readonly TimeSpan UsedCodeRetention = TimeSpan.FromMinutes(2);

    private readonly ITwoFactorCredentialRepository _credentials;
    private readonly IUsedTotpCodeRepository _usedCodes;
    private readonly ITotpProvider _totp;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IClock _clock;

    public RequestStepUpTokenCommandHandler(
        ITwoFactorCredentialRepository credentials,
        IUsedTotpCodeRepository usedCodes,
        ITotpProvider totp,
        IJwtTokenGenerator jwtTokenGenerator,
        IClock clock)
    {
        _credentials = credentials;
        _usedCodes = usedCodes;
        _totp = totp;
        _jwtTokenGenerator = jwtTokenGenerator;
        _clock = clock;
    }

    public async Task<StepUpTokenResult> Handle(RequestStepUpTokenCommand request, CancellationToken cancellationToken)
    {
        var credential = await _credentials.GetByUserIdAsync(request.UserId, cancellationToken);
        if (credential is null || !credential.IsEnabled)
            throw new TwoFactorNotEnabledException();

        var now = _clock.UtcNow;

        if (await _usedCodes.IsActiveAsync(request.UserId, request.Code, now, cancellationToken))
            throw new TotpCodeReplayedException();

        if (!_totp.VerifyCode(credential.Secret, request.Code))
            throw new InvalidTotpCodeException();

        _usedCodes.Add(new UsedTotpCode(UsedTotpCodeId.New(), request.UserId, request.Code, now, now + UsedCodeRetention));

        var token = _jwtTokenGenerator.GenerateStepUpToken(request.UserId, request.Operation);

        return new StepUpTokenResult(token.Value, token.ExpiresAt);
    }
}
