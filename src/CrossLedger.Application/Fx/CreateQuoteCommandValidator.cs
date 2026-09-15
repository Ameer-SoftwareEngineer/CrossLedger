using FluentValidation;

namespace CrossLedger.Application.Fx;

public sealed class CreateQuoteCommandValidator : AbstractValidator<CreateQuoteCommand>
{
    public CreateQuoteCommandValidator()
    {
        RuleFor(x => x.FromCurrency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.ToCurrency).NotEmpty().MaximumLength(3);
        RuleFor(x => x.ToCurrency).NotEqual(x => x.FromCurrency).WithMessage("A quote requires two different currencies.");
        RuleFor(x => x.Amount).GreaterThan(0m);
    }
}
