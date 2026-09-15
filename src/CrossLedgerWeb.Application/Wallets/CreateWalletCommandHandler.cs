using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Domain.Wallets;
using MediatR;

namespace CrossLedgerWeb.Application.Wallets;

public sealed class CreateWalletCommandHandler : IRequestHandler<CreateWalletCommand, CreateWalletResult>
{
    private readonly IWalletRepository _wallets;

    public CreateWalletCommandHandler(IWalletRepository wallets)
    {
        _wallets = wallets;
    }

    public Task<CreateWalletResult> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        var currency = Currency.From(request.Currency);
        var wallet = new Wallet(WalletId.New(), request.OwnerId, currency);

        _wallets.Add(wallet);

        return Task.FromResult(new CreateWalletResult(wallet.Id, wallet.OwnerId, wallet.Currency));
    }
}
