using FluentValidation;

namespace CrossLedgerWeb.Application.Transfers;

public sealed class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.QuoteId.Value).NotEmpty();
        RuleFor(x => x.SourceWalletId.Value).NotEmpty();
        RuleFor(x => x.TargetWalletId.Value).NotEmpty();

        RuleFor(x => x.TargetWalletId)
            .NotEqual(x => x.SourceWalletId)
            .WithMessage("Source and target wallet must be different.");

        RuleFor(x => x.SourceAmount).GreaterThan(0m);

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(128);
    }
}
