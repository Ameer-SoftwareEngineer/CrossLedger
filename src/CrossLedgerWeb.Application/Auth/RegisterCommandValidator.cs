using FluentValidation;

namespace CrossLedgerWeb.Application.Auth;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        // The authoritative complexity policy lives in Identity's PasswordOptions
        // (Infrastructure) - this just rejects an obviously-too-short password before
        // it reaches that layer at all.
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
