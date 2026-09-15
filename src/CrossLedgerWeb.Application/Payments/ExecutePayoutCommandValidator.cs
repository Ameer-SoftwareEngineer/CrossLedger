using FluentValidation;

namespace CrossLedgerWeb.Application.Payments;

public sealed class ExecutePayoutCommandValidator : AbstractValidator<ExecutePayoutCommand>
{
    public ExecutePayoutCommandValidator()
    {
        RuleFor(x => x.SourceWalletId.Value).NotEmpty();
        RuleFor(x => x.TargetCurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.DestinationCountry).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(128);
    }
}
