using FluentValidation;

namespace CrossLedgerWeb.Application.Auth;

public sealed class ConfirmTotpEnrollmentCommandValidator : AbstractValidator<ConfirmTotpEnrollmentCommand>
{
    public ConfirmTotpEnrollmentCommandValidator()
    {
        RuleFor(x => x.Secret).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches("^[0-9]+$");
    }
}
