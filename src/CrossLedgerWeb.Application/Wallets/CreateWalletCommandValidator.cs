using FluentValidation;

namespace CrossLedgerWeb.Application.Wallets;

public sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.OwnerId.Value).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
    }
}
