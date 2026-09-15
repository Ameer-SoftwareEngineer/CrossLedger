using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Exceptions;
using MediatR;

namespace CrossLedger.Application.Auth;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IIdentityService _identity;

    public RegisterCommandHandler(IIdentityService identity)
    {
        _identity = identity;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var outcome = await _identity.RegisterAsync(request.Email, request.Password, cancellationToken);

        if (!outcome.Succeeded)
            throw new RegistrationFailedException(outcome.Errors);

        return new RegisterResult(outcome.UserId!.Value, request.Email);
    }
}
