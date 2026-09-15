using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Exceptions;
using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using MediatR;

namespace CrossLedger.Application.Auth;

public sealed class ConfirmTotpEnrollmentCommandHandler : IRequestHandler<ConfirmTotpEnrollmentCommand, ConfirmTotpEnrollmentResult>
{
    private const int RecoveryCodeCount = 10;

    private readonly ITotpProvider _totp;
    private readonly ITwoFactorCredentialRepository _credentials;
    private readonly IRecoveryCodeRepository _recoveryCodes;
    private readonly IClock _clock;

    public ConfirmTotpEnrollmentCommandHandler(
        ITotpProvider totp,
        ITwoFactorCredentialRepository credentials,
        IRecoveryCodeRepository recoveryCodes,
        IClock clock)
    {
        _totp = totp;
        _credentials = credentials;
        _recoveryCodes = recoveryCodes;
        _clock = clock;
    }

    public Task<ConfirmTotpEnrollmentResult> Handle(ConfirmTotpEnrollmentCommand request, CancellationToken cancellationToken)
    {
        if (!_totp.VerifyCode(request.Secret, request.Code))
            throw new InvalidTotpCodeException();

        var now = _clock.UtcNow;

        // Only reachable once VerifyCode has succeeded - the secret is never persisted
        // before this point (specification 6.1's enrolment integrity).
        var credential = new TwoFactorCredential(TwoFactorCredentialId.New(), request.UserId, request.Secret, now);
        _credentials.Add(credential);

        var rawCodes = new List<string>(RecoveryCodeCount);
        var codeEntities = new List<RecoveryCode>(RecoveryCodeCount);
        for (var i = 0; i < RecoveryCodeCount; i++)
        {
            var raw = RecoveryCodeGenerator.GenerateCode();
            rawCodes.Add(raw);
            codeEntities.Add(new RecoveryCode(RecoveryCodeId.New(), request.UserId, RecoveryCodeGenerator.Hash(raw), now));
        }

        _recoveryCodes.AddRange(codeEntities);

        return Task.FromResult(new ConfirmTotpEnrollmentResult(rawCodes));
    }
}
