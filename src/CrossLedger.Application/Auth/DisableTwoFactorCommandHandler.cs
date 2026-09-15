using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Exceptions;
using MediatR;

namespace CrossLedger.Application.Auth;

public sealed class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand>
{
    private readonly ITwoFactorCredentialRepository _credentials;

    public DisableTwoFactorCommandHandler(ITwoFactorCredentialRepository credentials)
    {
        _credentials = credentials;
    }

    public async Task Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var credential = await _credentials.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new TwoFactorNotEnabledException();

        credential.Disable();
    }
}
