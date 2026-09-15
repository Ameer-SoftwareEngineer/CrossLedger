using CrossLedgerWeb.Application.Abstractions;
using MediatR;

namespace CrossLedgerWeb.Application.Auth;

/// <summary>Deliberately persists nothing: enrolment integrity (specification 6.1)
/// means the secret only reaches storage once ConfirmTotpEnrollmentCommand proves the
/// user can actually generate a valid code from it. The client holds the secret between
/// the two calls - it needs to anyway, to show the QR code / manual entry key.</summary>
public sealed class BeginTotpEnrollmentCommandHandler : IRequestHandler<BeginTotpEnrollmentCommand, BeginTotpEnrollmentResult>
{
    private const string Issuer = "CrossLedgerWeb";

    private readonly ITotpProvider _totp;

    public BeginTotpEnrollmentCommandHandler(ITotpProvider totp)
    {
        _totp = totp;
    }

    public Task<BeginTotpEnrollmentResult> Handle(BeginTotpEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var secret = _totp.GenerateSecret();
        var qrCodeUri = _totp.GenerateQrCodeUri(secret, request.Email, Issuer);

        return Task.FromResult(new BeginTotpEnrollmentResult(secret, qrCodeUri));
    }
}
