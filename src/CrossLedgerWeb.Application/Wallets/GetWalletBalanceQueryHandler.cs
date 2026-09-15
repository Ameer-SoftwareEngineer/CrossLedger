using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Exceptions;
using CrossLedgerWeb.Domain.ValueObjects;
using MediatR;

namespace CrossLedgerWeb.Application.Wallets;

public sealed class GetWalletBalanceQueryHandler : IRequestHandler<GetWalletBalanceQuery, Money>
{
    private readonly IWalletRepository _wallets;

    public GetWalletBalanceQueryHandler(IWalletRepository wallets)
    {
        _wallets = wallets;
    }

    public async Task<Money> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _wallets.GetByIdAsync(request.WalletId, cancellationToken)
            ?? throw new WalletNotFoundException(request.WalletId);

        return wallet.Balance;
    }
}
