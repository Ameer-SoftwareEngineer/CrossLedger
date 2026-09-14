using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Exceptions;
using CrossLedger.Domain.Ledger;
using CrossLedger.Domain.ValueObjects;
using MediatR;

namespace CrossLedger.Application.Transfers;

/// <summary>
/// Mirrors the request flow in specification section 7.1: load the quote and wallets,
/// let the domain post the four ledger entries, then commit atomically. Validation and
/// idempotency are handled by pipeline behaviours, not by this handler.
/// </summary>
public sealed class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, CreateTransferResult>
{
    private readonly IQuoteRepository _quotes;
    private readonly IWalletRepository _wallets;
    private readonly IFxSettlementWalletResolver _fxSettlementWallets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateTransferCommandHandler(
        IQuoteRepository quotes,
        IWalletRepository wallets,
        IFxSettlementWalletResolver fxSettlementWallets,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _quotes = quotes;
        _wallets = wallets;
        _fxSettlementWallets = fxSettlementWallets;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<CreateTransferResult> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var quote = await _quotes.GetByIdAsync(request.QuoteId, cancellationToken)
            ?? throw new QuoteNotFoundException(request.QuoteId);

        var sourceWallet = await _wallets.GetByIdAsync(request.SourceWalletId, cancellationToken)
            ?? throw new WalletNotFoundException(request.SourceWalletId);

        var targetWallet = await _wallets.GetByIdAsync(request.TargetWalletId, cancellationToken)
            ?? throw new WalletNotFoundException(request.TargetWalletId);

        var fxSettlementSource = await _fxSettlementWallets.GetSettlementWalletAsync(quote.FromCurrency, cancellationToken);
        var fxSettlementTarget = await _fxSettlementWallets.GetSettlementWalletAsync(quote.ToCurrency, cancellationToken);

        var now = _clock.UtcNow;
        var transferId = TransferId.New();
        var sourceAmount = new Money(request.SourceAmount, quote.FromCurrency);

        var entries = TransferPoster.Post(
            transferId, quote,
            sourceWallet, fxSettlementSource, fxSettlementTarget, targetWallet,
            sourceAmount, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var targetCreditEntry = entries[^1];

        return new CreateTransferResult(transferId, sourceAmount, targetCreditEntry.Amount, now);
    }
}
