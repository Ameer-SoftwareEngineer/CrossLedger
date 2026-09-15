using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Domain.ValueObjects;
using MediatR;

namespace CrossLedgerWeb.Application.Transfers;

public sealed record CreateTransferCommand(
    QuoteId QuoteId,
    WalletId SourceWalletId,
    WalletId TargetWalletId,
    decimal SourceAmount,
    string IdempotencyKey) : IRequest<CreateTransferResult>, IIdempotentRequest;
