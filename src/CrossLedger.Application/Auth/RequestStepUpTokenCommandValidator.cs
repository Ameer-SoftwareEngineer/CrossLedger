using FluentValidation;

namespace CrossLedger.Application.Auth;

public sealed class RequestStepUpTokenCommandValidator : AbstractValidator<RequestStepUpTokenCommand>
{
    public RequestStepUpTokenCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches("^[0-9]+$");
    }
}
